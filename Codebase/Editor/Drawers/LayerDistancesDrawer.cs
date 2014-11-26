using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(LayerDistances))]
public class LayerDistancesDrawer : PropertyDrawer{
	public bool expanded;
	public int drawn;
	public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
		if(this.expanded){
			return ((EditorGUIUtility.singleLineHeight+2) * this.drawn) + 16;
		}
		return base.GetPropertyHeight(property,label);
	}
    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		float singleLine = EditorGUIUtility.singleLineHeight;
		GUI.changed = false;
		EditorGUI.BeginProperty(position,label,property);
		position = position.SetHeight(singleLine);
		this.expanded = EditorGUI.Foldout(position,this.expanded,"Layer Cull Distances");
		if(this.expanded){
			EditorGUI.indentLevel += 1;
			this.drawn = 0;
			SerializedProperty valuesProperty = property.FindPropertyRelative("values");
			for(int index=0;index<32;index++){
				SerializedProperty current = valuesProperty.GetArrayElementAtIndex(index);
				string layerName = LayerMask.LayerToName(index);
				//if(layerName.IsEmpty()){layerName = "[Unnamed]";}
				if(!layerName.IsEmpty()){
					position = position.AddY(singleLine+2);
					current.floatValue = current.floatValue.DrawLabeled(position,new GUIContent(layerName));
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