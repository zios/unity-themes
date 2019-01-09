using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Drawers.LayerDistances{
	using Zios.Extensions;
	using Zios.Unity.Components.CameraAdvanced;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	[CustomPropertyDrawer(typeof(LayerDistances))]
	public class LayerDistancesDrawer : PropertyDrawer{
		public int drawn;
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
			if(EditorPref.Get<bool>("layerDistancesExpanded")){
				return ((EditorGUIUtility.singleLineHeight+2) * this.drawn) + 16;
			}
			return base.GetPropertyHeight(property,label);
		}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			EditorUI.Reset();
			float singleLine = EditorGUIUtility.singleLineHeight;
			area = area.SetHeight(singleLine);
			bool expanded = EditorPref.Get<bool>("layerDistancesExpanded");
			expanded = EditorGUI.Foldout(area,expanded,"Layer Cull Distances");
			EditorPref.Set<bool>("layerDistancesExpanded",expanded);
			if(expanded){
				EditorGUI.indentLevel += 1;
				this.drawn = 0;
				float[] values = property.FindPropertyRelative("values").GetObject<float[]>();
				for(int index=0;index<32;index++){
					string layerName = LayerMask.LayerToName(index);
					//if(layerName.IsEmpty()){layerName = "[Unnamed]";}
					if(!layerName.IsEmpty()){
						area = area.AddY(singleLine+2);
						values[index] = values[index].Draw(area,new GUIContent(layerName));
						this.drawn += 1;
					}
				}
				EditorGUI.indentLevel -= 1;
			}
		}
	}
}