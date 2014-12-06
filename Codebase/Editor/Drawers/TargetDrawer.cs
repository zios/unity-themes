using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(Target),true)]
public class TargetDrawer : PropertyDrawer{
	public static Dictionary<Target,bool?> toggled = new Dictionary<Target,bool?>();
	public static Dictionary<Target,bool> unfound = new Dictionary<Target,bool>();
    public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
		if(!area.InspectorValid()){return;}
		string skin = EditorGUIUtility.isProSkin ? "Dark" : "Light";
		GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skin + ".guiskin");
		GUI.changed = false;
		Target target = property.GetObject<Target>();
		Rect toggleRect = new Rect(area);
		Rect propertyRect = new Rect(area);
		float labelWidth = label.text.IsEmpty() ? 0 : EditorGUIUtility.labelWidth;
		propertyRect.x += labelWidth + 18;
		propertyRect.width -= labelWidth + 18;
		toggleRect.x += labelWidth;
		toggleRect.width = 18;
		if(!TargetDrawer.toggled.ContainsKey(target)){TargetDrawer.toggled[target] = null;}
		if(!TargetDrawer.unfound.ContainsKey(target)){TargetDrawer.unfound[target] = false;}
		bool toggleActive = TargetDrawer.toggled[target] ?? target.search.IsEmpty();
		TargetDrawer.toggled[target] = toggleActive.Draw(toggleRect,GUI.skin.GetStyle("TargetToggle"));
		bool toggled = (bool)TargetDrawer.toggled[target];
        EditorGUI.BeginProperty(area,label,property);
		if(toggleActive != toggled){
			target.search = "";
			target.DefaultSearch();
			if(toggled){
				target.direct = null;
				TargetDrawer.unfound[target] = true;
			}
		}
		label.DrawLabel(area,null,true);
		if(toggled){
			if(TargetDrawer.unfound[target]){target.direct = null;}
			GameObject direct = target.direct;
			target.direct = target.direct.DrawObject(propertyRect,true);
			if(target.direct != direct){
				TargetDrawer.unfound[target] = false;
			}
		}
		else{
			Rect textRect = propertyRect;
			string result = target.direct != null ? target.direct.GetPath().Trim("/") : "Not Found.";
			Vector2 textSize = GUI.skin.textField.CalcSize(new GUIContent(target.search));
			Vector2 subtleSize = GUI.skin.GetStyle("SubtleInfo").CalcSize(new GUIContent(result));
			float subtleX = propertyRect.x+propertyRect.width-subtleSize.x;
			float subtleWidth = subtleSize.x;
			float minimumX = propertyRect.x+textSize.x+3;
			if(subtleX < minimumX){
				subtleWidth -= (minimumX-subtleX);
				subtleX = minimumX;
			}
			propertyRect = propertyRect.SetX(subtleX).SetWidth(subtleWidth);
			EditorGUIUtility.AddCursorRect(propertyRect,MouseCursor.Zoom);
			if(!target.direct.IsNull() && propertyRect.Clicked(0)){
				Selection.activeGameObject = target.direct;
				Event.current.Use();
			}
			target.search = target.search.Draw(textRect);
			property.FindPropertyRelative("search").stringValue = target.search;
			result.DrawLabel(propertyRect,GUI.skin.GetStyle("SubtleInfo"));
		}
        EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
		if(GUI.changed){
			Utility.SetDirty(property.serializedObject.targetObject);
		}
    }
}