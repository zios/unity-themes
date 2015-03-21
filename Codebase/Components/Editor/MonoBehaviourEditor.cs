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
		public static float resumeHierarchyTime;
		public static bool hideAllDefault;
		public bool? hideDefault;
		public bool setup;
		public List<SerializedProperty> properties = new List<SerializedProperty>();
		public List<SerializedProperty> hidden = new List<SerializedProperty>();
		public Dictionary<SerializedProperty,Rect> area = new Dictionary<SerializedProperty,Rect>();
	    public override void OnInspectorGUI(){
			if(Application.isPlaying){
				this.DrawDefaultInspector();
				return;
			}
			if(!Event.current.IsUseful()){return;}
			this.SortDefaults();
			this.SortProperties();
			this.Setup();
			Type type = this.target.GetType();
			GUI.changed = false;
			foreach(var property in this.properties){
				bool isHidden = this.hidden.Contains(property);
				bool hideDefault = this.hideDefault != null ? (bool)this.hideDefault : MonoBehaviourEditor.hideAllDefault;
				if(hideDefault){
					object defaultValue = MonoBehaviourEditor.defaults[type][property.name];
					object currentValue = property.GetObject<object>();
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
					if(this.area.ContainsKey(property)){
						if(Event.current.shift){
							bool canHide = (this.properties.Count - this.hidden.Count) > 1;
							if(this.area[property].Clicked(0) && canHide){
								//Undo.RecordObject(this.serializedObject.targetObject,"Hide " + property.propertyPath);
								string path = "InspectorPropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
								EditorPrefs.SetBool(path,true);
								this.hidden.Add(property);
							}
							if(this.area[property].Clicked(1)){this.DrawHiddenMenu();}
						}
					}
					property.DrawLabeled();
					Rect area = GUILayoutUtility.GetLastRect();
					if(!area.IsEmpty()){this.area[property] = area;}
				}
			}		
			if(GUI.changed){
				this.serializedObject.ApplyModifiedProperties();
				this.serializedObject.Update();
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
			bool debugState = AttributeManager.debug;
			Type type = this.target.GetType();
			var defaults = MonoBehaviourEditor.defaults;
			if(!defaults.ContainsKey(type)){
				AttributeManager.debug = false;
				AttributeManager.disabled = true;
				Utility.PauseHierarchyUpdates();
				defaults.AddNew(type);
				var script = (MonoBehaviour)this.target;
				var component = script.gameObject.AddComponent(type);
				foreach(string name in component.ListVariables()){
					/*try{
						var behaviour = (DataMonoBehaviour)component;
						behaviour.Awake();
						defaults[type][name] = component.GetVariable(name);
					}
					catch{
						try{defaults[type][name] = component.GetVariable(name);}
						catch{}
					}*/
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
				AttributeManager.debug = debugState;
				AttributeManager.disabled = false;
				MonoBehaviourEditor.resumeHierarchyTime = Time.realtimeSinceStartup + 0.5f;
			}
			else if(Time.realtimeSinceStartup > MonoBehaviourEditor.resumeHierarchyTime){
				Utility.ResumeHierarchyUpdates();
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
			MenuFunction allDefaults = ()=>{MonoBehaviourEditor.hideAllDefault = !MonoBehaviourEditor.hideAllDefault;};
			MenuFunction localDefaults = ()=>{
				if(this.hideDefault == null){this.hideDefault = true;}
				else{this.hideDefault = !(bool)this.hideDefault;}
			};
			bool hideLocalDefault = this.hideDefault != null ? (bool)this.hideDefault : false;
			menu.AddItem(new GUIContent("Defaults/Show \u2044 Hide All"),MonoBehaviourEditor.hideAllDefault,allDefaults);
			menu.AddItem(new GUIContent("Defaults/Show \u2044 Hide Local"),hideLocalDefault,localDefaults);
			if(this.hidden.Count > 0){
				MenuFunction unhideAll = ()=>{this.hidden.Clear();};
				menu.AddItem(new GUIContent("Unhide/All"),false,unhideAll);
				menu.AddSeparator("Unhide/");
			}
			foreach(var property in this.hidden){
				SerializedProperty target = property;
				MenuFunction method = ()=>{
					string path = "InspectorPropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
					EditorPrefs.SetBool(path,false);
					this.hidden.Remove(target);
				};
				menu.AddItem(new GUIContent("Unhide/"+property.displayName),false,method);
			}
			menu.ShowAsContext();
			Event.current.Use();
		}
    }
}