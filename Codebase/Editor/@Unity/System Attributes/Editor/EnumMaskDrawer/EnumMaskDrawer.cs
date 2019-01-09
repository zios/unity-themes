using System;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.SystemAttributes.EnumMaskDrawer{
	using Zios.Extensions.Convert;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.EditorUI;
	using Zios.Unity.SystemAttributes;
	[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
	public class EnumMaskDrawer : PropertyDrawer{
		public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
			EditorUI.Reset();
			Enum value = property.GetObject<Enum>();
			value = value.DrawMask(position,label);
			property.intValue = value.ToInt();
			property.serializedObject.ApplyModifiedProperties();
		}
	}
}