using UnityEngine;
using UnityEditor;
using System.Collections;
[CustomPropertyDrawer(typeof(EventBool))]
public class EventBoolDrawer : PropertyDrawer{
    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		SerializedProperty value = property.FindPropertyRelative("value");
        Rect area = new Rect(position.x,position.y,position.width,position.height);
        EditorGUI.BeginProperty(position,label,property);
        EditorGUI.PropertyField(area,value,new GUIContent(label),true);
        EditorGUI.EndProperty();
    }
}