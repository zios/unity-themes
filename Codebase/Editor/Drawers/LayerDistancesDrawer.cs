using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(LayerDistances))]
public class LayerDistancesDrawer : PropertyDrawer{
	public int drawn;
	public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
		this.OnGUI(new Rect(-30000,-30000,0,0),property,label);
		if(EditorPrefs.GetBool("layerDistancesExpanded")){
			return ((EditorGUIUtility.singleLineHeight+2) * this.drawn) + 16;
		}
		return base.GetPropertyHeight(property,label);
	}
    public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
		if(!area.InspectorValid()){return;}
		float singleLine = EditorGUIUtility.singleLineHeight;
		GUI.changed = false;
		EditorGUI.BeginProperty(area,label,property);
		area = area.SetHeight(singleLine);
		bool expanded = EditorPrefs.GetBool("layerDistancesExpanded");
		expanded = EditorGUI.Foldout(area,expanded,"Layer Cull Distances");
		EditorPrefs.SetBool("layerDistancesExpanded",expanded);
		if(expanded){
			EditorGUI.indentLevel += 1;
			this.drawn = 0;
			SerializedProperty valuesProperty = property.FindPropertyRelative("values");
			for(int index=0;index<32;index++){
				SerializedProperty current = valuesProperty.GetArrayElementAtIndex(index);
				string layerName = LayerMask.LayerToName(index);
				//if(layerName.IsEmpty()){layerName = "[Unnamed]";}
				if(!layerName.IsEmpty()){
					area = area.AddY(singleLine+2);
					current.floatValue = current.floatValue.DrawLabeled(area,new GUIContent(layerName));
					this.drawn += 1;
				}
			}
			EditorGUI.indentLevel -= 1;
		}
		EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
		if(GUI.changed){
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
    }
}