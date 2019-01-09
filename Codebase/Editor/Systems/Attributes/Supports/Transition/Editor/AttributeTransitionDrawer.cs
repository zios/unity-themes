using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Attributes.Supports.Transition{
	using Zios.Attributes.Supports.Transition;
	using Zios.Events;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Editor.Extensions;
	//asm Zios.Attributes.Supports;
	//asm Zios.Shortcuts;
	//asm Zios.Supports.Transition;
	//asm Zios.Unity.Shortcuts;
	[CustomPropertyDrawer(typeof(AttributeTransition))]
	public class AttributeTransitionDrawer : PropertyDrawer{
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
			var hash = property.GetObject<AttributeTransition>().path;
			if(EditorPref.Get<bool>(hash)){return EditorGUIUtility.singleLineHeight*5+8;}
			return base.GetPropertyHeight(property,label);
		}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			EditorUI.Reset();
			AttributeTransition transition = property.GetObject<AttributeTransition>();
			var spacing = area.height = EditorGUIUtility.singleLineHeight;
			if(!transition.time.isSetup){return;}
			if("Transition".ToLabel().DrawFoldout(area,transition.path,null,true)){
				GUI.changed = false;
				EditorGUI.indentLevel += 1;
				transition.time.Set(transition.time.Get().Draw(area.AddY(spacing+2),"Time"));
				transition.speed.Set(transition.speed.Get().Draw(area.AddY(spacing*2+4),"Speed"));
				transition.acceleration = transition.acceleration.Draw(area.AddY(spacing*3+6),"Acceleration");
				transition.deceleration = transition.deceleration.Draw(area.AddY(spacing*4+8),"Deceleration");
				EditorGUI.indentLevel -= 1;
				if(GUI.changed){
					property.serializedObject.targetObject.DelayEvent("On Validate",1);
					transition.Setup(transition.path,transition.parent);
				}
			}
		}
	}
}