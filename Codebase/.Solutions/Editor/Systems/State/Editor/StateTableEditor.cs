using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.State{
	using Zios.State;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[CustomEditor(typeof(StateTable),true)]
	public class StateTableEditor : StateBehaviourEditor{
		public override void OnInspectorGUI(){
			EditorUI.Reset();
			this.SetupColors();
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