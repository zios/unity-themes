using Zios;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
namespace Zios{
    [CustomEditor(typeof(MonoBehaviour),true)][CanEditMultipleObjects]
    public class MonoBehaviourEditor : Editor{
		public List<SerializedProperty> properties = new List<SerializedProperty>();
		public List<SerializedProperty> hidden = new List<SerializedProperty>();
	    public override void OnInspectorGUI(){
			//this.DrawDefaultInspector();
			if(Event.current.type == EventType.ScrollWheel){return;}
			this.DrawHiddenMenu();
			if(this.properties.Count < 1){
				var property = this.serializedObject.GetIterator();
				property.NextVisible(true);
				while(property.NextVisible(false)){
					var realProperty = this.serializedObject.FindProperty(property.propertyPath);
					this.properties.Add(realProperty);
				}
			}
			foreach(var property in this.properties){
				if(!hidden.Contains(property)){
					try{property.Draw(property.displayName);}
					catch{continue;}
					Rect lastRect = GUILayoutUtility.GetLastRect();
					if(Event.current.shift && lastRect.Clicked(0)){
						this.hidden.Add(property);
					}
				}
			}
			if(GUI.changed){
				this.serializedObject.ApplyModifiedProperties();
			}
	    }
		public void DrawHiddenMenu(){}
    }
}