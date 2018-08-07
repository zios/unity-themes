using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace Zios.Unity.Editor.Supports.Reference{
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.EditorUI;
	public class ReferenceDrawer<Type,Data> : PropertyDrawer{
		public static Dictionary<object,float> height = new Dictionary<object,float>();
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
			var target = property.GetObject<Type>();
			return height.ContainsKey(target) ? height[target] : 16;
		}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			if(area.width == 1){return;}
			EditorGUI.BeginProperty(area,label,property);
			this.OnGUI(area,property.propertyPath,property.GetObject<Type>(),label);
			EditorGUI.EndProperty();
		}
		public void OnGUI(Rect area,string key,Type value,GUIContent label){
			if(area.width == 1){return;}
			EditorUI.Reset();
			height[value] = area.height = 16;
			GUI.enabled = false;
			var size = value.Call("Get").As<Data[]>().Draw(area,key,label.text,1);
			GUI.enabled = true;
			height[value] += size.y-area.y-16;
		}
	}
}