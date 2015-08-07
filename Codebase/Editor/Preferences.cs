using UnityEngine;
using UnityEditor;
namespace Zios{
	public class Preferences{
		[PreferenceItem("Zios")]
		public static void Main(){
			GUIContent fastInspectorContent = new GUIContent("Turbo Inspector (Experimental)");
			GUIContent alwaysUpdateContent = new GUIContent("Always Update");
			fastInspectorContent.tooltip = "Prevents offscreen attributes/components from being drawn in inspectors. ";
			fastInspectorContent.tooltip += "Currently has issues with multiple inspectors visible and erratic nudging position offset issues while scrolling.";
			alwaysUpdateContent.tooltip = "Forces the scene view to repaint every frame.  Huge performance cost, but will allow shaders based on time to update in realtime.";
			bool fastInspector = EditorPrefs.GetBool("MonoBehaviourEditor-FastInspector").Draw(fastInspectorContent);
			bool alwaysUpdate = EditorPrefs.GetBool("SceneSettings-AlwaysUpdate").Draw(alwaysUpdateContent);
			if(GUI.changed){
				EditorPrefs.SetBool("MonoBehaviourEditor-FastInspector",fastInspector);
				EditorPrefs.SetBool("SceneSettings-AlwaysUpdate",alwaysUpdate);
			}
		}
	}
}