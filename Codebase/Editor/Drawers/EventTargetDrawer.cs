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
		GameObject target = eventTarget.target.Get();
		label.DrawLabel(labelRect,null,true);
		string eventType = eventTarget.mode == EventMode.Listeners ? "Listen" : "Caller";
		bool hasEvents = eventType == "Listen" ? !Events.HasEvents("Listen",target) : !Events.HasEvents("Caller",target);
		bool toggleActive = this.targetMode.ContainsKey(eventTarget) ? this.targetMode[eventTarget] : !eventTarget.name.IsEmpty();
		this.targetMode[eventTarget] = toggleActive.Draw(valueRect.SetWidth(16),GUI.skin.GetStyle("CheckmarkToggle"));
		valueRect = valueRect.Add(18,0,-18,0);
		if(!this.targetMode[eventTarget]){
			property.FindPropertyRelative("target").Draw(valueRect);
		}
		else if(!target.IsNull() && hasEvents){
			string error = "No <b>"+eventType+"</b> events found for target -- " + target.name;
			error.DrawLabel(valueRect,GUI.skin.GetStyle("WarningLabel"));
		}
		else{
			List<string> events = eventType == "Listen" ? Events.GetEvents("Listen",target) : Events.GetEvents("Caller",target);
			if(events.Count > 0){
				events.Sort();
				events = events.OrderBy(item=>item.Contains("/")).ToList();
				events.RemoveAll(item=>item.StartsWith("@"));
				int index = events.IndexOf(eventTarget.name);
				bool missing = false;
				if(index == -1){
					missing = true;
					events.Insert(0,"[Missing] " + eventTarget.name);
					index = 0;
				}
				index = events.Draw(valueRect,index);
				if(!missing || index != 0){
					eventTarget.name = events[index];
				}
			}
			else{
				string error = "No global <b>"+eventType+"</b> events exist.";
				error.Draw(valueRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
        EditorGUI.EndProperty();
		property.serializedObject.ApplyModifiedProperties();
		if(GUI.changed){
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
    }
}