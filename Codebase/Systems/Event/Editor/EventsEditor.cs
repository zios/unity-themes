using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Zios.UI{
    [CustomEditor(typeof(Events))]
    public class EventsEditor : MonoBehaviourEditor{
	    public override void OnInspectorGUI(){
			Events.disabled = Events.disabled.Draw("Disabled");
			Events.debugScope = (EventDebugScope)Events.debugScope.DrawMask("Debug Scope");
			Events.debug = (EventDebug)Events.debug.DrawMask("Debug");
			if("Listeners".DrawFoldout(true)){
				EditorGUI.indentLevel += 1;
				var listeners = Events.listeners.GroupBy(x=>Events.GetTargetName(x.target)).ToDictionary(x=>x.Key,x=>x.ToList());
				var labelStyle = GUI.skin.label.FixedWidth(200);
				var valueStyle = GUI.skin.label.FixedWidth(250);
				var checkStyle = GUI.skin.toggle.FixedWidth(32);
				foreach(var item in listeners){
					if(item.Key.DrawFoldout(true)){
						EditorGUI.indentLevel += 1;
						foreach(var listener in item.Value){
							GUILayout.BeginHorizontal();
							listener.name.DrawLabel(labelStyle,true);
							Events.GetMethodName(listener.method).DrawLabel(valueStyle);
							listener.permanent.Draw(null,checkStyle);
							listener.isStatic.Draw(null,checkStyle);
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