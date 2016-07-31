using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Interface;
	using Events;
	[CustomEditor(typeof(Event))]
	public class EventsEditor : Editor{
		public Dictionary<string,List<EventListener>> listeners = new Dictionary<string,List<EventListener>>();
		public void BuildListeners(){
			this.listeners = Event.listeners.GroupBy(x=>Event.GetTargetName(x.target)).ToDictionary(x=>x.Key,x=>x.ToList());
		}
		public override void OnInspectorGUI(){
			Event.disabled = (EventDisabled)Event.disabled.DrawMask("Disabled");
			Event.debugScope = (EventDebugScope)Event.debugScope.DrawMask("Debug Scope");
			Event.debug = (EventDebug)Event.debug.DrawMask("Debug");
			if("Listeners".ToLabel().DrawFoldout()){
				EditorGUI.indentLevel += 1;
				if(this.listeners.Count != Event.listeners.Count){this.BuildListeners();}
				var labelStyle = GUI.skin.label.FixedWidth(200);
				var valueStyle = GUI.skin.label.FixedWidth(350);
				var checkStyle = GUI.skin.toggle.FixedWidth(16);
				foreach(var item in this.listeners){
					if(item.Key.ToLabel().DrawFoldout()){
						EditorGUI.indentLevel += 1;
						foreach(var listener in item.Value){
							GUILayout.BeginHorizontal();
							listener.name.ToLabel().DrawLabel(labelStyle,true);
							Event.GetMethodName(listener.method).ToLabel().DrawLabel(valueStyle);
							listener.isStatic.Draw(null,checkStyle);
							listener.permanent.Draw(null,checkStyle);
							listener.unique.Draw(null,checkStyle);
							GUILayout.EndHorizontal();
						}
						EditorGUI.indentLevel -= 1;
					}
				}
				EditorGUI.indentLevel -= 1;
			}
			Event.eventHistory.Draw("Event History");
		}
	}
}