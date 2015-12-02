using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Zios.UI{
	[CustomPropertyDrawer(typeof(ListBool))]
	public class ListBoolDrawer : PropertyDrawer{
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			string[] names = new string[]{"X","Y","Z","W"};
			List<bool> data = property.GetObject<ListBool>().value;
			Rect labelRect = area.SetWidth(EditorGUIUtility.labelWidth);
			Rect valueRect = area.Add(labelRect.width,0,-labelRect.width,0);
			label.DrawLabel(labelRect,null,true);
			for(int index=0;index<data.Count;++index){
				data[index] = data[index].Draw(valueRect.AddX((index*30)).SetWidth(30));
				names[index].DrawLabel(valueRect.Add(14+(index*30)));
			}
		}
	}
}