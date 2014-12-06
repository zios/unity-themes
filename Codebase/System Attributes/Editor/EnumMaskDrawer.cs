using System;
using UnityEngine;
using UnityEditor;
[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
public class EnumMaskDrawer : PropertyDrawer{
	public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		EditorGUI.BeginProperty(position,label,property);
		Enum value = property.GetObject<Enum>();
		value = value.DrawLabeledMask(position,label,null);
		property.intValue = value.ToInt();
		EditorGUI.EndProperty();
	}
}