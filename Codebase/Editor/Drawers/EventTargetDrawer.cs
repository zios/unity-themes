using UnityEngine;
using UnityEditor;
using Zios;
using System.Linq;
using System.Collections.Generic;
namespace Zios.UI{
    [CustomPropertyDrawer(typeof(EventTarget))]
    public class EventTargetDrawer : PropertyDrawer{
	    public Dictionary<EventTarget,bool> targetMode = new Dictionary<EventTarget,bool>();
        public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
		    if(!Attribute.ready){
			    EditorGUI.ProgressBar(area,AttributeManager.percentLoaded,"Updating");
			    return;
		    }
		    string skin = EditorGUIUtility.isProSkin ? "Dark" : "Light";
		    GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skin + ".guiskin");
		    Rect labelRect = area.SetWidth(EditorGUIUtility.labelWidth);
		    Rect valueRect = area.Add(labelRect.width,0,-labelRect.width,0);
		    EventTarget eventTarget = property.GetObject<EventTarget>();
		    string eventName = eventTarget.name;
		    GameObject target = eventTarget.target.Get();
		    label.DrawLabel(labelRect,null,true);
		    string eventType = eventTarget.mode == EventMode.Listeners ? "Listen" : "Caller";
		    bool hasEvents = eventType == "Listen" ? !Events.HasEvents("Listen",target) : !Events.HasEvents("Caller",target);
		    bool toggleActive = this.targetMode.ContainsKey(eventTarget) ? this.targetMode[eventTarget] : !eventTarget.name.IsEmpty();
		    this.targetMode[eventTarget] = toggleActive.Draw(valueRect.SetWidth(16),"",GUI.skin.GetStyle("CheckmarkToggle"));
		    valueRect = valueRect.Add(18,0,-18,0);
		    if(!this.targetMode[eventTarget]){
			    property.FindPropertyRelative("target").Draw(valueRect);
		    }
		    else if(!target.IsNull() && hasEvents){
			    string error = "No <b>"+eventType+"</b> events found for target -- " + target.name;
			    error.DrawLabel(valueRect,GUI.skin.GetStyle("WarningLabel"));
		    }
		    else{
			    List<string> events = eventType == "Listen" ? Events.GetEventNames("Listen",target) : Events.GetEventNames("Caller",target);
			    if(events.Count > 0){
				    events.Sort();
				    events = events.OrderBy(item=>item.Contains("/")).ToList();
				    events.RemoveAll(item=>item.StartsWith("@"));
				    int index = events.IndexOf(eventName);
				    bool missing = false;
				    if(index == -1){
					    missing = true;
					    events.Insert(0,"[Missing] " + eventName);
					    index = 0;
				    }
				    index = events.Draw(valueRect,index);
				    if(!missing || index != 0){
					    eventTarget.name.Set(events[index]);
				    }
			    }
			    else{
				    string error = "No global <b>"+eventType+"</b> events exist.";
				    error.Draw(valueRect,"",GUI.skin.GetStyle("WarningLabel"));
			    }
		    }
        }
    }
}