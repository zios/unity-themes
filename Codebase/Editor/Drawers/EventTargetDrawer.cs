using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
[CustomPropertyDrawer(typeof(EventTarget))]
public class EventTargetDrawer : PropertyDrawer{
	public Dictionary<EventTarget,bool> targetMode = new Dictionary<EventTarget,bool>();
    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
		string skin = EditorGUIUtility.isProSkin ? "Dark" : "Light";
		GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skin + ".guiskin");
		Rect labelRect = position.SetWidth(EditorGUIUtility.labelWidth);
		Rect valueRect = position.Add(labelRect.width,0,-labelRect.width,0);
		GUI.changed = false;
        EditorGUI.BeginProperty(position,label,property);
		EventTarget eventTarget = property.GetObject<EventTarget>();
		Target target = eventTarget.target;
		label.Draw(labelRect,null,true);
		bool toggleActive = this.targetMode.ContainsKey(eventTarget) ? this.targetMode[eventTarget] : target.direct != null;
		if(target.direct == null){toggleActive = false;}
		this.targetMode[eventTarget] = toggleActive.Draw(valueRect.SetWidth(16),GUI.skin.GetStyle("CheckmarkToggle"));
		valueRect = valueRect.Add(18,0,-18,0);
		if(!this.targetMode[eventTarget]){
			SerializedProperty targetProperty = property.FindPropertyRelative("target");
			targetProperty.Draw(valueRect);
			return;
		}
		if(target.direct != null){
			if(Events.objectEvents.ContainsKey(target.direct)){
				List<string> events = Events.GetEvents(target.direct);
				events.Sort();
				events = events.OrderBy(item=>item.Contains("/")).ToList();
				events.RemoveAll(item=>item.StartsWith("@"));
				int index = events.IndexOf(eventTarget.name);
				if(index == -1){index = 0;}
				index = events.Draw(valueRect,index);
				eventTarget.name = events[index];
			}
			else{
				"No Events Found.".Draw(valueRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
        EditorGUI.EndProperty();
		if(GUI.changed){
			property.serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
    }
}