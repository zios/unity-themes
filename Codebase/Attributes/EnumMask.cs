using UnityEngine;
using System;
#if UNITY_EDITOR
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
#endif
public class EnumMaskAttribute : PropertyAttribute{}