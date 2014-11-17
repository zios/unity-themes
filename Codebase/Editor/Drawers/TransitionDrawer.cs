using UnityEngine;
using UnityEditor;
[CustomPropertyDrawer(typeof(Transition))]
public class TransitionDrawer : PropertyDrawer{
    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		SerializedProperty duration = property.FindPropertyRelative("duration");
		SerializedProperty delay = property.FindPropertyRelative("delayStart");
		SerializedProperty curve = property.FindPropertyRelative("curve");
		Rect labelRect = position.SetWidth(EditorGUIUtility.labelWidth);
		Rect valueRect = position.Add(labelRect.width,0,-labelRect.width,0);
		EditorGUI.BeginProperty(position,label,property);
		label.DrawLabel(labelRect,null,true);
		duration.Draw(valueRect.SetWidth(35));
		"seconds".DrawLabel(valueRect.AddX(37).SetWidth(50));
		delay.Draw(valueRect.AddX(90).SetWidth(35));
		"delay".DrawLabel(valueRect.AddX(127).SetWidth(40));
		curve.Draw(valueRect.Add(169,0,-169,0));
		EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
    }
}