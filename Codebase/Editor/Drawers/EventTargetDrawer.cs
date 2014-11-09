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
		label.DrawLabel(labelRect,null,true);
		bool toggleActive = this.targetMode.ContainsKey(eventTarget) ? this.targetMode[eventTarget] : !eventTarget.name.IsEmpty();
		this.targetMode[eventTarget] = toggleActive.Draw(valueRect.SetWidth(16),GUI.skin.GetStyle("CheckmarkToggle"));
		valueRect = valueRect.Add(18,0,-18,0);
		if(!this.targetMode[eventTarget]){
			property.FindPropertyRelative("target").Draw(valueRect);
		}
		else if(!target.direct.IsNull() && !Events.objectEvents.ContainsKey(target.direct)){
			string error = "No events found for target -- " + target.direct.name;
			error.DrawLabel(valueRect,GUI.skin.GetStyle("WarningLabel"));
		}
		else{
			List<string> events = Events.GetEvents(target.direct);
			if(events.Count > 0){
				events.Sort();
				events = events.OrderBy(item=>item.Contains("/")).ToList();
				events.RemoveAll(item=>item.StartsWith("@"));
				int index = events.IndexOf(eventTarget.name);
				if(index == -1){index = 0;}
				index = events.Draw(valueRect,index);
				eventTarget.name = events[index];
			}
			else{
				string error = "No global events exist.";
				error.Draw(valueRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
        EditorGUI.EndProperty();
		if(GUI.changed){
			property.serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
    }
}