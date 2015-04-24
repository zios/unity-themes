using Zios;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios{
    [CustomEditor(typeof(MonoBehaviour),true)][CanEditMultipleObjects]
    public class MonoBehaviourEditor : Editor{
		public static Dictionary<Type,Dictionary<string,object>> defaults = new Dictionary<Type,Dictionary<string,object>>();
		public static float resumeHierarchyTime = -1;
		public static bool hideAllDefault;
		public bool hideDefault;
		public bool setup;
		public List<SerializedProperty> properties = new List<SerializedProperty>();
		public List<SerializedProperty> hidden = new List<SerializedProperty>();
		public Dictionary<SerializedProperty,Rect> propertyArea = new Dictionary<SerializedProperty,Rect>();
		public Rect area;
		public Rect areaStart;
	    public override void OnInspectorGUI(){
			if(Utility.IsPlaying() || Application.isLoadingLevel){
				this.DrawDefaultInspector();
				return;
			}
			if(!Event.current.IsUseful()){return;}
			if(this.target.As<MonoBehaviour>().IsPrefab()){return;}
			try{this.areaStart = GUILayoutUtility.GetRect(0,0);}
			catch{}
			this.serializedObject.Update();
			MonoBehaviourEditor.hideAllDefault = EditorPrefs.GetBool("MonoBehaviourEditor-HideAllDefault",false);
			this.hideDefault = EditorPrefs.GetBool("MonoBehaviourEditor-"+this.target.GetInstanceID()+"HideDefault",false);
			bool hideDefault = MonoBehaviourEditor.hideAllDefault ? MonoBehaviourEditor.hideAllDefault : this.hideDefault;
			if(hideDefault){this.SortDefaults();}
			this.SortProperties();
			this.Setup();
			Type type = this.target.GetType();
			bool changed = false;
			bool showAll = false;
			bool showAdvanced = EditorPrefs.GetBool("InspectorAdvanced");
			bool showInternal = EditorPrefs.GetBool("InspectorInternal");
			Vector2 mousePosition = Event.current.mousePosition;
			if(Event.current.alt){
				showAll = this.area.Contains(mousePosition);
				this.Repaint();
			}
			foreach(var property in this.properties){
				string[] attributes = this.serializedObject.targetObject.ListAttributes(property.name).Select(x=>x.GetType().Name).ToArray();
				bool isInternal = attributes.Contains("InternalAttribute");
				bool isAdvanced = attributes.Contains("AdvancedAttribute");
				bool isReadOnly = isInternal || attributes.Contains("ReadOnlyAttribute");
				bool isHidden = !showAll && this.hidden.Contains(property);
				if(isAdvanced && !showAdvanced){isHidden = true;}
				if(isInternal && !showInternal){isHidden = true;}
				if(!showAll && hideDefault){
					object defaultValue = MonoBehaviourEditor.defaults[type][property.name];
					object currentValue = property.GetObject<object>();
					if(defaultValue.IsNull()){continue;}
					if(currentValue is AttributeFloat){currentValue = ((AttributeFloat)currentValue).Get();}
					if(currentValue is AttributeInt){currentValue = ((AttributeInt)currentValue).Get();}
					if(currentValue is AttributeBool){currentValue = ((AttributeBool)currentValue).Get();}
					if(currentValue is AttributeString){currentValue = ((AttributeString)currentValue).Get();}
					if(currentValue is AttributeVector3){currentValue = ((AttributeVector3)currentValue).Get();}
					if(currentValue is AttributeGameObject){currentValue = ((AttributeGameObject)currentValue).Get();}
					bool isDefault = defaultValue.Equals(currentValue);
					if(isDefault){isHidden = true;}
				}
				if(!isHidden){
					if(this.propertyArea.ContainsKey(property)){
						if(Event.current.shift){
							bool canHide = (this.properties.Count - this.hidden.Count) > 1;
							if(this.propertyArea[property].Clicked(0) && canHide){
								string path = "InspectorPropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
								EditorPrefs.SetBool(path,true);
								this.hidden.Add(property);
								this.Repaint();
							}
							if(this.propertyArea[property].Clicked(1)){this.DrawMenu();}
						}
						if(!this.propertyArea[property].InspectorValid()){continue;}
					}
					try{
						if(isReadOnly){GUI.enabled = false;}
						GUI.changed = false;
						EditorGUI.BeginProperty(this.propertyArea.AddNew(property),new GUIContent(property.displayName),property);
						property.DrawLabeled();
						EditorGUI.EndProperty();
						changed = changed || GUI.changed;
						if(isReadOnly){GUI.enabled = true;}
						Rect area = GUILayoutUtility.GetLastRect();
						if(!area.IsEmpty()){this.propertyArea[property] = area;}
					}
					catch{}
				}
			}		
			try{
				Rect areaEnd = GUILayoutUtility.GetRect(0,0);
				if(!areaEnd.IsEmpty()){
					this.area = this.areaStart.AddY(-15);
					this.area.height = (areaEnd.y - this.areaStart.y) + 15;
				}
			}
			catch{}
			if(changed){
				this.serializedObject.ApplyModifiedProperties();
				this.serializedObject.targetObject.CallMethod("OnValidate");
				Utility.SetDirty(this.serializedObject.targetObject);
			}
	    }
		public void Setup(){
			if(this.properties.Count > 0 && !this.setup){
				foreach(var property in this.properties){
					string path = "InspectorPropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
					if(EditorPrefs.GetBool(path,false)){
						this.hidden.Add(property);
					}
				}
				this.setup = true;
			}
		}
		public void SortDefaults(){
			Type type = this.target.GetType();
			var defaults = MonoBehaviourEditor.defaults;
			if(!defaults.ContainsKey(type)){
				Events.Pause("On Hierarchy Changed");
				Events.disabled = true;
				AttributeManager.disabled = true;
				Utility.delayPaused = true;
				defaults.AddNew(type);
				var script = (MonoBehaviour)this.target;
				var component = script.gameObject.AddComponent(type);
				foreach(string name in component.ListVariables()){
					try{
						object defaultValue = component.GetVariable(name);
						if(defaultValue is AttributeFloat){defaultValue = ((AttributeFloat)defaultValue).Get();}
						if(defaultValue is AttributeInt){defaultValue = ((AttributeInt)defaultValue).Get();}
						if(defaultValue is AttributeBool){defaultValue = ((AttributeBool)defaultValue).Get();}
						if(defaultValue is AttributeString){defaultValue = ((AttributeString)defaultValue).Get();}
						if(defaultValue is AttributeVector3){defaultValue = ((AttributeVector3)defaultValue).Get();}
						if(defaultValue is AttributeGameObject){defaultValue = ((AttributeGameObject)defaultValue).Get();}
						defaults[type][name] = defaultValue;
					}
					catch{}
				}
				Utility.Destroy(component);
				Utility.delayPaused = false;
				Events.disabled = false;
				AttributeManager.disabled = false;
				MonoBehaviourEditor.resumeHierarchyTime = Time.realtimeSinceStartup + 0.5f;
			}
			else if(MonoBehaviourEditor.resumeHierarchyTime != -1 && Time.realtimeSinceStartup > MonoBehaviourEditor.resumeHierarchyTime){
				MonoBehaviourEditor.resumeHierarchyTime = -1;
				Events.Resume("On Hierarchy Changed");
			}
		}
		public void SortProperties(){
			if(this.properties.Count < 1){
				var target = this.serializedObject.targetObject;
				var property = this.serializedObject.GetIterator();
				property.NextVisible(true);
				while(property.NextVisible(false)){
					var realProperty = this.serializedObject.FindProperty(property.propertyPath);
					this.properties.Add(realProperty);
				}
				this.properties = this.properties.OrderBy(x=>target.HasAttribute(x.name,typeof(InternalAttribute))).ToList();
			}
		}
		public void DrawMenu(){
			GenericMenu menu = new GenericMenu();
			bool showAdvanced = EditorPrefs.GetBool("InspectorAdvanced");
			bool showInternal = EditorPrefs.GetBool("InspectorInternal");
			menu.AddItem(new GUIContent("Advanced"),showAdvanced,()=>EditorPrefs.SetBool("InspectorAdvanced",!showAdvanced));
			menu.AddItem(new GUIContent("Internal"),showInternal,()=>EditorPrefs.SetBool("InspectorInternal",!showInternal));
			MenuFunction hideAllDefaults = ()=>{
				MonoBehaviourEditor.hideAllDefault = !MonoBehaviourEditor.hideAllDefault;
				EditorPrefs.SetBool("MonoBehaviourEditor-HideAllDefault",MonoBehaviourEditor.hideAllDefault);
			};
			MenuFunction hideLocalDefaults = ()=>{
				this.hideDefault = !this.hideDefault;
				EditorPrefs.SetBool("MonoBehaviourEditor-"+this.target.GetInstanceID()+"HideDefault",this.hideDefault);
			};
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Defaults/Hide All"),MonoBehaviourEditor.hideAllDefault,hideAllDefaults);
			menu.AddItem(new GUIContent("Defaults/Hide Local"),this.hideDefault,hideLocalDefaults);
			if(this.hidden.Count > 0){
				MenuFunction unhideAll = ()=>{
					foreach(var property in this.hidden){
						string path = "InspectorPropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
						EditorPrefs.SetBool(path,false);
					}
					this.hidden.Clear();
				};
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Unhide/All"),false,unhideAll);
				foreach(var property in this.hidden){
					SerializedProperty target = property;
					MenuFunction unhide = ()=>{
						string path = "InspectorPropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
						EditorPrefs.SetBool(path,false);
						this.hidden.Remove(target);
					};
					menu.AddItem(new GUIContent("Unhide/"+property.displayName),false,unhide);
				}
			}
			menu.ShowAsContext();
			Event.current.Use();
		}
    }
}