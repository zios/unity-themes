using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Zios.UI{
    [CustomEditor(typeof(Events))]
    public class EventsEditor : Editor{
		public Dictionary<string,List<EventListener>> listeners;
		public void BuildListeners(){
			this.listeners = Events.listeners.GroupBy(x=>Events.GetTargetName(x.target)).ToDictionary(x=>x.Key,x=>x.ToList());
		}
	    public override void OnInspectorGUI(){
			//Events.Add("On Events Changed",()=>Utility.EditorDelayCall(this.BuildListeners)).SetUnique();
			Events.disabled = (EventDisabled)Events.disabled.DrawMask("Disabled");
			Events.debugScope = (EventDebugScope)Events.debugScope.DrawMask("Debug Scope");
			Events.debug = (EventDebug)Events.debug.DrawMask("Debug");
			if("Listeners".DrawFoldout(true)){
				EditorGUI.indentLevel += 1;
				if(this.listeners == null){this.BuildListeners();}
				var labelStyle = GUI.skin.label.FixedWidth(200);
				var valueStyle = GUI.skin.label.FixedWidth(350);
				var checkStyle = GUI.skin.toggle.FixedWidth(16);
				foreach(var item in this.listeners){
					if(item.Key.DrawFoldout(true)){
						EditorGUI.indentLevel += 1;
						foreach(var listener in item.Value){
							GUILayout.BeginHorizontal();
							listener.name.DrawLabel(labelStyle,true);
							Events.GetMethodName(listener.method).DrawLabel(valueStyle);
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
			Events.eventHistory.Draw("Event History");
	    }
    }
}