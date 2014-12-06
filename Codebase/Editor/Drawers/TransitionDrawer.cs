using Zios;
using UnityEngine;
using UnityEditor;
[CustomPropertyDrawer(typeof(Transition))]
public class TransitionDrawer : PropertyDrawer{
    public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
		if(!area.InspectorValid()){return;}
		GUI.changed = false;
		Transition transition = property.GetObject<Transition>();
		float durationValue = transition.duration.Get();
		float delayValue = transition.delayStart.Get();
		AnimationCurve curveValue = transition.curve;
		Rect labelRect = area.SetWidth(EditorGUIUtility.labelWidth);
		Rect valueRect = area.Add(labelRect.width,0,-labelRect.width,0);
		EditorGUI.BeginProperty(area,label,property);
		label.DrawLabel(labelRect,null,true);
		durationValue = durationValue.Draw(valueRect.SetWidth(35));
		"seconds".DrawLabel(valueRect.AddX(37).SetWidth(50));
		delayValue = delayValue.Draw(valueRect.AddX(90).SetWidth(35));
		"delay".DrawLabel(valueRect.AddX(127).SetWidth(40));
		curveValue = transition.curve.Draw(valueRect.Add(169,0,-169,0));
		EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
		if(GUI.changed){
			transition.duration.Set(durationValue);
			transition.delayStart.Set(delayValue);
			transition.curve = curveValue;
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
    }
}