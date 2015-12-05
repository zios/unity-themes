using UnityEditor;
using UnityEngine;
namespace Zios.UI{
	[CustomEditor(typeof(StateTable),true)]
	public class StateTableEditor : StateMonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.DrawBreakdown();
			string message = "Click here to open the State Window.";
			message.DrawHelp();
			Rect area = GUILayoutUtility.GetLastRect();
			EditorGUIUtility.AddCursorRect(area,MouseCursor.Link);
			if(area.Clicked()){
				StateWindow.Begin();
			}
		}
	}
}