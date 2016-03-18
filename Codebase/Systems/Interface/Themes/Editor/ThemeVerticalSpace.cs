using UnityEditor;
using UnityEngine;
namespace Zios{
	public class ThemeVerticalSpace : PropertyDrawer{
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){return base.GetPropertyHeight(property,label) * 1.5f;}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){typeof(EditorGUI).CallMethod("DefaultPropertyField",new object[]{area,property,label});}
	}

}