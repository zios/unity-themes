using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(ListBool))]
public class ListBoolDrawer : PropertyDrawer{
    public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
		if(!area.InspectorValid()){return;}
		string[] names = new string[]{"X","Y","Z","W"};
		object dataObject = property.GetObject<object>();
		Rect labelRect = area.SetWidth(EditorGUIUtility.labelWidth);
		Rect valueRect = area.Add(labelRect.width,0,-labelRect.width,0);
		GUI.changed = false;
		EditorGUI.BeginProperty(area,label,property);
		if(dataObject is ListBool){
			List<bool> data = ((ListBool)dataObject).value;
			EditorGUI.LabelField(labelRect,label);
			for(int index=0;index<data.Count;++index){
				data[index] = data[index].Draw(valueRect.AddX((index*30)).SetWidth(30));
				names[index].DrawLabel(valueRect.Add(14+(index*30)));
			}
		}
		EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
		if(GUI.changed){
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
    }
}