using System;
using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Interface;
	[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
	public class EnumMaskDrawer : PropertyDrawer{
		public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
			Enum value = property.GetObject<Enum>();
			value = value.DrawMask(position,label,null,true);
			property.intValue = value.ToInt();
			property.serializedObject.ApplyModifiedProperties();
		}
	}
}