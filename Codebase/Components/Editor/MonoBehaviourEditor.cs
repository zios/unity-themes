using Zios;
using System;
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
		public Dictionary<object,bool> layoutReady = new Dictionary<object,bool>();
		public List<SerializedProperty> properties = new List<SerializedProperty>();
		public List<SerializedProperty> hidden = new List<SerializedProperty>();
		public Dictionary<SerializedProperty,Rect> propertyArea = new Dictionary<SerializedProperty,Rect>();
		public Rect area;
		public Rect areaStart;
	    public override void OnInspectorGUI(){
			if(Application.isPlaying){
				this.DrawDefaultInspector();
				return;
			}
			bool drawAllowed = Event.current.type.Has("Layout") || (Event.current.type.Has("Repaint") && this.layoutReady.AddNew("AreaStart"));
			if(drawAllowed){
				this.layoutReady["AreaStart"] = Event.current.type.Has("Layout");
				this.areaStart = GUILayoutUtility.GetRect(0,0);
			}
			if(!Event.current.IsUseful()){return;}
			if(this.target.As<MonoBehaviour>().IsPrefab()){return;}
			MonoBehaviourEditor.hideAllDefault = EditorPrefs.GetBool("MonoBehaviourEditor-HideAllDefault",false);
			this.hideDefault = EditorPrefs.GetBool("MonoBehaviourEditor-"+this.target.GetInstanceID()+"HideDefault",false);
			this.serializedObject.Update();
			bool hideDefault = MonoBehaviourEditor.hideAllDefault ? MonoBehaviourEditor.hideAllDefault : this.hideDefault;
			if(hideDefault){this.SortDefaults();}
			this.SortProperties();
			this.Setup();
			Type type = this.target.GetType();
			GUI.changed = false;
			bool showAll = false;
			Vector2 mousePosition = Event.current.mousePosition;
			if(Event.current.control){
				showAll = this.area.Contains(mousePosition);
				this.Repaint();
			}
			foreach(var property in this.properties){
				bool isHidden = !showAll && this.hidden.Contains(property);
				drawAllowed = Event.current.type.Has("Layout") || (Event.current.type.Has("Repaint") && this.layoutReady.AddNew(property));
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
							if(this.propertyArea[property].Clicked(1)){this.DrawHiddenMenu();}
						}
					}
					if(drawAllowed){
						this.layoutReady[property] = Event.current.type.Has("Layout");
						try{property.DrawLabeled();}
						catch{}
						Rect area = GUILayoutUtility.GetLastRect();
						if(!area.IsEmpty()){this.propertyArea[property] = area;}
					}
				}
			}		
			drawAllowed = Event.current.type.Has("Layout") || (Event.current.type.Has("Repaint") && this.layoutReady.AddNew("AreaEnd"));
			if(drawAllowed){
				this.layoutReady["AreaEnd"] = Event.current.type.Has("Layout");
				try{
					Rect areaEnd = GUILayoutUtility.GetRect(0,0);
					if(!areaEnd.IsEmpty()){
						this.area = this.areaStart.AddY(-15);
						this.area.height = (areaEnd.y - this.areaStart.y) + 15;
					}
				}
				catch{}
			}
			if(GUI.changed){
				this.serializedObject.ApplyModifiedProperties();
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
				var property = this.serializedObject.GetIterator();
				property.NextVisible(true);
				while(property.NextVisible(false)){
					var realProperty = this.serializedObject.FindProperty(property.propertyPath);
					this.properties.Add(realProperty);
				}
			}
		}
		public void DrawHiddenMenu(){
			GenericMenu menu = new GenericMenu();
			MenuFunction hideAllDefaults = ()=>{
				MonoBehaviourEditor.hideAllDefault = !MonoBehaviourEditor.hideAllDefault;
				EditorPrefs.SetBool("MonoBehaviourEditor-HideAllDefault",MonoBehaviourEditor.hideAllDefault);
			};
			MenuFunction hideLocalDefaults = ()=>{
				this.hideDefault = !this.hideDefault;
				EditorPrefs.SetBool("MonoBehaviourEditor-"+this.target.GetInstanceID()+"HideDefault",this.hideDefault);
			};
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
				menu.AddItem(new GUIContent("Unhide/All"),false,unhideAll);
				menu.AddSeparator("Unhide/");
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