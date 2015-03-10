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
		public static bool hideAllDefault;
		public bool hideDefault;
		public bool setup;
		public List<SerializedProperty> properties = new List<SerializedProperty>();
		public List<SerializedProperty> hidden = new List<SerializedProperty>();
		public Dictionary<SerializedProperty,Rect> area = new Dictionary<SerializedProperty,Rect>();
	    public override void OnInspectorGUI(){
			//this.DrawDefaultInspector();
			if(!Event.current.IsUseful()){return;}
			this.SortDefaults();
			this.SortProperties();
			this.Setup();
			foreach(var property in this.properties){
				if(!this.hidden.Contains(property)){
					if(this.area.ContainsKey(property) && Event.current.shift){
						bool canHide = (this.properties.Count - this.hidden.Count) > 1;
						if(this.area[property].Clicked(0) && canHide){
							//Undo.RecordObject(this.serializedObject.targetObject,"Hide " + property.propertyPath);
							string path = "InspectorPropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
							EditorPrefs.SetBool(path,true);
							this.hidden.Add(property);
						}
						if(this.area[property].Clicked(1)){this.DrawHiddenMenu();}
					}
					property.DrawLabeled();
					Rect area = GUILayoutUtility.GetLastRect();
					if(!area.IsEmpty()){this.area[property] = area;}
				}
			}
			if(GUI.changed){
				this.serializedObject.ApplyModifiedProperties();
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
			MenuFunction localDefaults = ()=>{this.hideDefault = !this.hideDefault;};
			menu.AddItem(new GUIContent("Defaults/Show \u2044 Hide All"),false,allDefaults);
			menu.AddItem(new GUIContent("Defaults/Show \u2044 Hide Local"),false,localDefaults);
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