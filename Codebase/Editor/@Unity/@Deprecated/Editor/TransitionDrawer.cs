using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Deprecated{
	using Zios.Attributes.Deprecated;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Editor.Extensions;
	//asm Zios.Attributes.Supports;
	[CustomPropertyDrawer(typeof(Transition))]
	public class OldTransitionDrawer : PropertyDrawer{
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			EditorUI.Reset();
			Transition transition = property.GetObject<Transition>();
			float durationValue = transition.duration.Get();
			float delayValue = transition.delayStart.Get();
			AnimationCurve curveValue = transition.curve;
			Rect labelRect = area.SetWidth(EditorGUIUtility.labelWidth);
			Rect valueRect = area.Add(labelRect.width,0,-labelRect.width,0);
			label.ToLabel().DrawLabel(labelRect,null,true);
			durationValue = durationValue.Draw(valueRect.SetWidth(35));
			"seconds".ToLabel().DrawLabel(valueRect.AddX(37).SetWidth(50));
			delayValue = delayValue.Draw(valueRect.AddX(90).SetWidth(35));
			"delay".ToLabel().DrawLabel(valueRect.AddX(127).SetWidth(40));
			curveValue = transition.curve.Draw(valueRect.Add(169,0,-169,0));
			if(GUI.changed){
				transition.duration.Set(durationValue);
				transition.delayStart.Set(delayValue);
				transition.curve = curveValue;
			}
		}
	}
}