using Zios;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios.UI{
    [CustomEditor(typeof(MonoBehaviour),true)][CanEditMultipleObjects]
    public class MonoBehaviourEditor : Editor{
		public static Dictionary<Type,Dictionary<string,object>> defaults = new Dictionary<Type,Dictionary<string,object>>();
		public static float resumeHierarchyTime = -1;
		public static Dictionary<Editor,bool> offScreen = new Dictionary<Editor,bool>();
		public bool hideDefault;
		public bool setup;
		public List<SerializedProperty> properties = new List<SerializedProperty>();
		public List<SerializedProperty> hidden = new List<SerializedProperty>();
		public Dictionary<SerializedProperty,Rect> propertyArea = new Dictionary<SerializedProperty,Rect>();
		public Dictionary<SerializedProperty,bool> propertyVisible = new Dictionary<SerializedProperty,bool>();
		public Rect area;
		public Rect areaStart;
		public Rect areaEnd;
		public bool areaBegan;
		public bool showAll;
		public bool visible = true;
		public Method dirtyEvent;
	    public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
			if(this.target is MonoBehaviour && this.target.As<MonoBehaviour>().IsPrefab()){return;}
			this.BeginArea();
			bool fastInspector = EditorPrefs.GetBool("MonoBehaviourEditor-FastInspector");
			/*if(fastInspector && MonoBehaviourEditor.offScreen.ContainsKey(this)){
				GUILayout.Space(this.area.height);
				this.CheckChanges();
				return;
			}*/
			bool hideAllDefault = EditorPrefs.GetBool("MonoBehaviourEditor-HideAllDefault",false);
			this.hideDefault = EditorPrefs.GetBool("MonoBehaviourEditor-"+this.target.GetInstanceID()+"HideDefault",false);
			bool hideDefault = hideAllDefault || this.hideDefault;
			if(hideDefault){this.SortDefaults();}
			this.serializedObject.Update();
			this.SortProperties();
			this.Setup();
			Type type = this.target.GetType();
			bool changed = false;
			bool showAdvanced = EditorPrefs.GetBool("MonoBehaviourEditor-Advanced");
			bool showInternal = EditorPrefs.GetBool("MonoBehaviourEditor-Internal");
			EditorGUILayout.BeginVertical();
			foreach(var property in this.properties){
				string[] attributes = this.serializedObject.targetObject.ListAttributes(property.name).Select(x=>x.GetType().Name).ToArray();
				bool isInternal = attributes.Contains("InternalAttribute");
				bool isAdvanced = attributes.Contains("AdvancedAttribute");
				bool isReadOnly = isInternal || attributes.Contains("ReadOnlyAttribute");
				bool isHidden = !this.showAll && this.hidden.Contains(property);
				if(isAdvanced && !showAdvanced){isHidden = true;}
				if(isInternal && !showInternal){isHidden = true;}
				object currentValue = property.GetObject<object>();
				bool hasDefault = MonoBehaviourEditor.defaults.ContainsKey(type) && MonoBehaviourEditor.defaults[type].ContainsKey(property.name);
				if(!this.showAll && hideDefault && hasDefault){
					object defaultValue = MonoBehaviourEditor.defaults[type][property.name];
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
					bool hasArea = this.propertyArea.ContainsKey(property);
					if(hasArea){
						if(Event.current.shift){
							bool canHide = (this.properties.Count - this.hidden.Count) > 1;
							if(this.propertyArea[property].Clicked(0) && canHide){
									string path = "InspectorPropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
									EditorPrefs.SetBool(path,true);
									this.hidden.Add(property);
							}
							if(this.propertyArea[property].Clicked(1)){this.DrawMenu();}
						}
						if(fastInspector && this.propertyVisible.ContainsKey(property) && !this.propertyVisible[property]){
							GUILayout.Space(this.propertyArea[property].height);
							continue;
						}
					}
					string propertyName = currentValue is Attribute ? currentValue.As<Attribute>().info.name : "";
					if(isReadOnly){GUI.enabled = false;}
					GUI.changed = false;
					EditorGUILayout.BeginVertical();
					if(hasArea){EditorGUI.BeginProperty(this.propertyArea[property],new GUIContent(property.displayName),property);}
					property.Draw(propertyName);
					if(hasArea){EditorGUI.EndProperty();}
					EditorGUILayout.EndVertical();
					changed = changed || GUI.changed;
					if(isReadOnly){GUI.enabled = true;}
					if(Event.current.type == EventType.Repaint){
						Rect area = GUILayoutUtility.GetLastRect();
						if(!area.IsEmpty()){this.propertyArea[property] = area.AddHeight(2);}
					}
				}
			}
			EditorGUILayout.EndVertical();
			this.EndArea();
			if(changed){
				this.serializedObject.ApplyModifiedProperties();
				//this.serializedObject.targetObject.CallMethod("OnValidate");
				Utility.SetDirty(this.serializedObject.targetObject,false,true);
			}
			this.CheckChanges();
	    }
		public void CheckChanges(){
			if(Event.current.type == EventType.Repaint){
				bool fastInspector = EditorPrefs.GetBool("MonoBehaviourEditor-FastInspector");
				Vector2 mousePosition = Event.current.mousePosition;
				this.showAll = Event.current.alt && this.area.Contains(mousePosition);
				if(this.dirtyEvent != null){
					this.dirtyEvent();
					this.dirtyEvent = null;
				}
				bool needsRepaint = false;
				if(fastInspector){
					foreach(var property in this.properties){
						if(this.propertyArea.ContainsKey(property)){
							bool valid = this.propertyArea[property].InInspectorWindow();
							if(valid != this.propertyVisible.AddNew(property)){
								this.propertyVisible[property] = valid;
								needsRepaint = true;
							}
						}
					}
					/*if(!this.area.IsEmpty() && !this.area.InInspectorWindow()){
						MonoBehaviourEditor.offScreen[this] = true;
					}*/
				}
				if(this.showAll || needsRepaint){this.Repaint();}
			}
		}
		public void BeginArea(){
			//if(this.areaBegan){return;}
			Rect areaStart = GUILayoutUtility.GetRect(0,0);
			if(!areaStart.IsEmpty() && this.areaStart != areaStart){
				this.AddDirty(()=>{
					this.areaStart = areaStart;
					this.propertyArea.Clear();
					MonoBehaviourEditor.offScreen.Clear();
					this.Repaint();
				});
			}
			this.areaBegan = true;
		}
		public void EndArea(){
			Rect areaEnd = GUILayoutUtility.GetRect(0,0);
			if(!areaEnd.IsEmpty() && this.areaEnd != areaEnd){
				this.AddDirty(()=>{
					this.area = this.areaStart;
					this.area.height = (areaEnd.y - this.areaStart.y);
				});
			}
			this.areaBegan = false;
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
			if(!(this.target is MonoBehaviour)){return;}
			if(!defaults.ContainsKey(type)){
				Events.Pause("On Hierarchy Changed");
				var state = Events.disabled;
				Events.disabled = (EventDisabled)(-1);
				AttributeManager.disabled = true;
				//Utility.delayPaused = true;
				defaults.AddNew(type);
				var script = (MonoBehaviour)this.target;
				var component = script.gameObject.AddComponent(type);
				foreach(var item in component.GetVariables()){
					try{
						string name = item.Key;
						object defaultValue = item.Value;
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
				//Utility.delayPaused = false;
				Events.disabled = state;
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
				if(target.IsNull()){return;}
				var property = this.serializedObject.GetIterator();
				property.NextVisible(true);
				while(property.NextVisible(false)){
					var realProperty = this.serializedObject.FindProperty(property.propertyPath);
					this.properties.Add(realProperty);
				}
				this.properties = this.properties.OrderBy(x=>target.HasAttribute(x.name,typeof(InternalAttribute))).ToList();
			}
		}
		public void AddDirty(Method method){this.dirtyEvent += method;}
		public void DrawMenu(){
			GenericMenu menu = new GenericMenu();
			MenuFunction toggleAdvanced = ()=>Utility.ToggleEditorPref("MonoBehaviourEditor-Advanced");
			MenuFunction toggleInternal = ()=>Utility.ToggleEditorPref("MonoBehaviourEditor-Internal");
			MenuFunction hideAllDefaults = ()=>Utility.ToggleEditorPref("MonoBehaviourEditor-HideAllDefault");
			MenuFunction hideLocalDefaults = ()=>{
				this.hideDefault = !this.hideDefault;
				EditorPrefs.SetBool("MonoBehaviourEditor-"+this.target.GetInstanceID()+"HideDefault",this.hideDefault);
			};
			menu.AddItem(new GUIContent("Advanced"),EditorPrefs.GetBool("MonoBehaviourEditor-Advanced"),toggleAdvanced);
			menu.AddItem(new GUIContent("Internal"),EditorPrefs.GetBool("MonoBehaviourEditor-Internal"),toggleInternal);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Defaults/Hide All"),EditorPrefs.GetBool("MonoBehaviourEditor-HideAllDefault"),hideAllDefaults);
			menu.AddItem(new GUIContent("Defaults/Hide Local"),this.hideDefault,hideLocalDefaults);
			if(this.hidden.Count > 0){
				MenuFunction unhideAll = ()=>{
					foreach(var property in this.hidden){
						string path = "MonoBehaviourEditor-PropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
						EditorPrefs.SetBool(path,false);
					}
					this.hidden.Clear();
				};
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Unhide/All"),false,unhideAll);
				foreach(var property in this.hidden){
					SerializedProperty target = property;
					MenuFunction unhide = ()=>{
						string path = "MonoBehaviourEditor-PropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
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
