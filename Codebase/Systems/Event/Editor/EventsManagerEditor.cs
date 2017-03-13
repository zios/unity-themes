using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Interface;
	using Event;
	[CustomEditor(typeof(EventsManager))]
	public class EventsEditor : MonoBehaviourEditor{
		public Dictionary<string,List<EventListener>> listeners = new Dictionary<string,List<EventListener>>();
		public void BuildListeners(){
			this.listeners = Events.listeners.GroupBy(x=>Events.GetTargetName(x.target)).ToDictionary(x=>x.Key,x=>x.ToList());
		}
		public override void OnInspectorGUI(){;
			this.title = "Events";
			this.header = this.header ?? FileManager.GetAsset<Texture2D>("EventsIcon.png");
			base.OnInspectorGUI();
			if(GUI.changed){this.target.As<EventsManager>().Update();}
			EditorUI.Reset();
			if("Listeners".ToLabel().DrawFoldout()){
				EditorGUI.indentLevel += 1;
				if(this.listeners.Count != Events.listeners.Count){this.BuildListeners();}
				foreach(var item in this.listeners){
					if(item.Key.ToLabel().DrawFoldout()){
						EditorGUI.indentLevel += 1;
						foreach(var listener in item.Value){
							GUILayout.BeginHorizontal();
							listener.name.ToLabel().Layout(200).DrawLabel();
							Events.GetMethodName(listener.method).ToLabel().Layout(250).DrawLabel(null,false);
							listener.isStatic.Layout(16).Draw(null,null,false);
							listener.permanent.Layout(16).Draw(null,null,false);
							listener.unique.Layout(16).Draw(null,null,false);
							GUILayout.EndHorizontal();
						}
						EditorGUI.indentLevel -= 1;
					}
				}
				EditorGUI.indentLevel -= 1;
			}
			Events.eventHistory.Draw("Event History");
		}
		[MenuItem("Zios/Settings/Events")]
		public static void Select(){
			Selection.activeObject = FileManager.GetAsset<EventsManager>("EventsManager.asset",false) ?? Utility.CreateSingleton("Assets/Settings/EventsManager");
		}
	}
}