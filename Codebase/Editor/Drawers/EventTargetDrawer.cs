using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Interface;
	using Attributes;
	using Events;
	[CustomPropertyDrawer(typeof(EventTarget))]
	public class EventTargetDrawer : PropertyDrawer{
		public bool targeted = true;
		public bool manual;
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			if(!Attribute.ready){
				EditorGUI.ProgressBar(area,AttributeManager.percentLoaded,"Updating");
				return;
			}
			string skin = EditorGUIUtility.isProSkin || EditorPrefs.GetBool("EditorTheme-Dark",false) ? "Dark" : "Light";
			GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skin + ".guiskin");
			Rect labelRect = area.SetWidth(EditorGUIUtility.labelWidth);
			Rect valueRect = area.Add(labelRect.width,0,-labelRect.width,0);
			EventTarget eventTarget = property.GetObject<EventTarget>();
			string eventName = eventTarget.name;
			GameObject target = eventTarget.target.Get();
			label.ToLabel().DrawLabel(labelRect,null,true);
			if(target.IsNull()){this.targeted = false;}
			string targetLabel = this.targeted ? "+" : "-";
			string manualLabel = this.manual ? "M" : "S";
			var buttonArea = valueRect.SetWidth(16);
			if(targetLabel.ToLabel().DrawButton(buttonArea)){this.targeted = !this.targeted;}
			if(manualLabel.ToLabel().DrawButton(buttonArea.AddX(18))){this.manual = !this.manual;}
			valueRect = valueRect.Add(36,0,-36,0);
			if(!this.targeted){
				property.FindPropertyRelative("target").Draw(valueRect);
				return;
			}
			if(!this.manual){
				string eventType = eventTarget.mode == EventMode.Listeners ? "Listen" : "Caller";
				bool hasEvents = eventType == "Listen" ? Event.HasListeners(target) : Event.HasCallers(target);
				if(!hasEvents){
					string error = "";
					if(!target.IsNull()){error = "No <b>"+eventType+"</b> events found for target -- " + target.name;}
					if(target.IsNull()){error = "No global <b>"+eventType+"</b> events exist.";}
					error.ToLabel().DrawLabel(valueRect,GUI.skin.GetStyle("WarningLabel"));
					return;
				}
				List<string> events = eventType == "Listen" ? Event.GetEventNames("Listen",target) : Event.GetEventNames("Caller",target);
				events.Sort();
				events = events.OrderBy(item=>item.Contains("/")).ToList();
				events.RemoveAll(item=>item.StartsWith("@"));
				int index = eventName.IsEmpty() ? 0 : events.IndexOf(eventName);
				bool missing = index == -1;
				if(index == -1){
					events.Insert(0,"[Missing] " + eventName);
					index = 0;
				}
				index = events.Draw(valueRect,index);
				if(!missing || index != 0){
					eventTarget.name.Set(events[index]);
				}
				return;
			}
			string name = eventTarget.name.Get().Draw(valueRect);
			eventTarget.name.Set(name);
			if(GUI.changed){
				property.serializedObject.targetObject.DelayEvent("On Validate",1);
			}
		}
	}
}