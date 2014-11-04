using UnityEngine;
using UnityEditor;
[CustomPropertyDrawer(typeof(Transition))]
public class TransitionDrawer : PropertyDrawer{
    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		int indent = EditorGUI.indentLevel;
		SerializedProperty duration = property.FindPropertyRelative("duration");
		SerializedProperty delay = property.FindPropertyRelative("delayStart");
		SerializedProperty curve = property.FindPropertyRelative("curve");
		Rect labelRect = position.SetWidth(EditorGUIUtility.labelWidth);
		Rect valueRect = position.Add(labelRect.width,0,-labelRect.width,0);
		EditorGUI.BeginProperty(position,label,property);
		EditorGUI.LabelField(labelRect,label);
		EditorGUI.indentLevel = 0;
		EditorGUI.PropertyField(valueRect.SetWidth(35),duration,GUIContent.none);
		EditorGUI.LabelField(valueRect.AddX(37).SetWidth(50),"seconds");
		EditorGUI.PropertyField(valueRect.AddX(90).SetWidth(35),delay,GUIContent.none);
		EditorGUI.LabelField(valueRect.AddX(127).SetWidth(40),"delay");
		EditorGUI.PropertyField(valueRect.Add(169,0,-169,0),curve,GUIContent.none);
		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
    }
}