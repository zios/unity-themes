using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using UnityObject = UnityEngine.Object;
namespace Zios{
	/*[CustomPropertyDrawer(typeof(object),true)]
	public class ThemeVerticalSpace : PropertyDrawer{
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){return base.GetPropertyHeight(property,label) * Themes.verticalSpacing;}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){typeof(EditorGUI).CallMethod("DefaultPropertyField",new object[]{area,property,label});}
	}
	[CustomPropertyDrawer(typeof(PropertyAttribute),true)] public class PropertyVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(SerializableAttribute),true)] public class SerializableVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(SerializedObject),true)] public class SerializedObjectVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(SerializedProperty),true)] public class SerializedPropertyVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(UnityObject),true)] public class UnityObjectVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(int),true)] public class IntVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(string),true)] public class StringVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(float),true)] public class FloatVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(bool),true)] public class BoolVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(Enumerable),true)] public class ListVerticalSpace : ThemeVerticalSpace{}
	[CustomPropertyDrawer(typeof(Enum),true)] public class EnumVerticalSpace : ThemeVerticalSpace{}*/
}