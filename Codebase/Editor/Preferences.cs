using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Interface;
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
			bool fastInspector = EditorPrefs.GetBool("MonoBehaviourEditor-FastInspector").Draw(fastInspectorHelp);
			bool alwaysUpdateShaders = EditorPrefs.GetBool("EditorSettings-AlwaysUpdateShaders").Draw(alwaysUpdateShadersHelp);
			bool alwaysUpdateParticles = EditorPrefs.GetBool("EditorSettings-AlwaysUpdateParticles").Draw(alwaysUpdateParticlesHelp);
			float particleUpdateRange = EditorPrefs.GetFloat("EditorSettings-ParticleUpdateRange",100).Draw(particleUpdateRangeHelp);
			if(GUI.changed){
				EditorPrefs.SetBool("MonoBehaviourEditor-FastInspector",fastInspector);
				EditorPrefs.SetBool("EditorSettings-AlwaysUpdateShaders",alwaysUpdateShaders);
				EditorPrefs.SetBool("EditorSettings-AlwaysUpdateParticles",alwaysUpdateParticles);
				EditorPrefs.SetFloat("EditorSettings-ParticleUpdateRange",particleUpdateRange);
			}
		}
	}
}