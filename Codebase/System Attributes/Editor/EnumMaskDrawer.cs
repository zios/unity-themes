using System;
using UnityEngine;
using UnityEditor;
namespace Zios.UI{
	[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
    public class EnumMaskDrawer : PropertyDrawer{
	    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		    Enum value = property.GetObject<Enum>();
		    value = value.DrawMask(position,label,null);
		    property.intValue = value.ToInt();
			property.serializedObject.ApplyModifiedProperties();
		}
	}
}
