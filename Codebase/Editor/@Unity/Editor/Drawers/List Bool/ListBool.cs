using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Drawers.ListBool{
	using Zios.Extensions;
	using Zios.Supports.NonGeneric;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	[CustomPropertyDrawer(typeof(ListBool))]
	public class ListBoolDrawer : PropertyDrawer{
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			EditorUI.Reset();
			string[] names = new string[]{"X","Y","Z","W"};
			List<bool> data = property.GetObject<ListBool>().value;
			Rect labelRect = area.SetWidth(EditorGUIUtility.labelWidth);
			Rect valueRect = area.Add(labelRect.width,0,-labelRect.width,0);
			label.ToLabel().DrawLabel(labelRect,null,true);
			for(int index=0;index<data.Count;++index){
				data[index] = data[index].Draw(valueRect.AddX((index*30)).SetWidth(30));
				names[index].ToLabel().DrawLabel(valueRect.Add(14+(index*30)));
			}
		}
	}
}