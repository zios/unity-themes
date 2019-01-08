using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Menus{
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.EditorUI;
	public class Preferences{
		[PreferenceItem("Zios")]
		public static void Main(){
			GUIContent fastInspectorHelp = new GUIContent("Turbo Inspector (Experimental)");
			GUIContent alwaysUpdateShadersHelp = new GUIContent("Always Update Shaders");
			GUIContent alwaysUpdateParticlesHelp = new GUIContent("Always Update Particles");
			GUIContent particleUpdateRangeHelp = new GUIContent("Always Update Particles (Range)");
			fastInspectorHelp.tooltip = "Prevents offscreen attributes/components from being drawn in inspectors. ";
			fastInspectorHelp.tooltip += "Currently has issues with multiple inspectors visible and erratic nudging position offset issues while scrolling.";
			alwaysUpdateShadersHelp.tooltip = "Forces the scene view to repaint every frame.  Huge performance cost, but will allow shaders based on time to update in realtime.";
			alwaysUpdateParticlesHelp.tooltip = "Forces the scene view to repaint every frame.  Huge performance cost, but will continually ensure particles are simulated.";
			particleUpdateRangeHelp.tooltip = "Range at which editor-updated particles will simulate.  Higher values will cost more performance.";
			if("Other".ToLabel().DrawFoldout("Zios.Preferences.Other")){
				EditorGUI.indentLevel += 1;
				bool fastInspector = EditorPref.Get<bool>("MonoBehaviourEditor-FastInspector").Draw(fastInspectorHelp);
				bool alwaysUpdateShaders = EditorPref.Get<bool>("EditorSettings-AlwaysUpdateShaders").Draw(alwaysUpdateShadersHelp);
				bool alwaysUpdateParticles = EditorPref.Get<bool>("EditorSettings-AlwaysUpdateParticles").Draw(alwaysUpdateParticlesHelp);
				float particleUpdateRange = EditorPref.Get<float>("EditorSettings-ParticleUpdateRange",100).Draw(particleUpdateRangeHelp);
				if(GUI.changed){
					EditorPref.Set<bool>("MonoBehaviourEditor-FastInspector",fastInspector);
					EditorPref.Set<bool>("EditorSettings-AlwaysUpdateShaders",alwaysUpdateShaders);
					EditorPref.Set<bool>("EditorSettings-AlwaysUpdateParticles",alwaysUpdateParticles);
					EditorPref.Set<float>("EditorSettings-ParticleUpdateRange",particleUpdateRange);
				}
				EditorGUI.indentLevel -= 1;
			}
		}
	}
}