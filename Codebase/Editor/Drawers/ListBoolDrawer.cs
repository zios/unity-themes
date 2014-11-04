using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(ListBool))]
public class ListBoolDrawer : PropertyDrawer{
    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		string[] names = new string[]{"X","Y","Z","W"};
		object dataObject = property.GetObject<object>();
		Rect labelRect = position.SetWidth(EditorGUIUtility.labelWidth);
		Rect valueRect = position.Add(labelRect.width,0,-labelRect.width,0);
		GUI.changed = false;
		EditorGUI.BeginProperty(position,label,property);
		if(dataObject is ListBool){
			List<bool> data = (List<bool>)dataObject;
			EditorGUI.LabelField(labelRect,label);
			valueRect = valueRect.AddX(-15);
			for(int index=0;index<data.Count;++index){
				data[index] = EditorGUI.Toggle(valueRect.AddX((index*30)).SetWidth(30),data[index]);
				EditorGUI.LabelField(valueRect.Add(14+(index*30)),names[index]);
			}
		}
		EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
		if(GUI.changed){
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
    }
}