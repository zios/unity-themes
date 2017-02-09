using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Interface;
	public class Preferences{
		[PreferenceItem("Zios")]
		public static void Main(){
			GUIContent disableHooksHelp = new GUIContent("Disable Hooks");
			GUIContent hideHooksHelp = new GUIContent("Hide Hooks");
			GUIContent temporaryHooksHelp = new GUIContent("Temporary Hooks");
			GUIContent fastInspectorHelp = new GUIContent("Turbo Inspector (Experimental)");
			GUIContent alwaysUpdateShadersHelp = new GUIContent("Always Update Shaders");
			GUIContent alwaysUpdateParticlesHelp = new GUIContent("Always Update Particles");
			GUIContent particleUpdateRangeHelp = new GUIContent("Always Update Particles (Range)");
			disableHooksHelp.tooltip = "Toggle whether @Main hooks will be created";
			hideHooksHelp.tooltip = "Toggle whether @Main hooks are visible in the hierarchy.";
			temporaryHooksHelp.tooltip = "Toggle whether @Main hooks are saved to scenes or not.";
			fastInspectorHelp.tooltip = "Prevents offscreen attributes/components from being drawn in inspectors. ";
			fastInspectorHelp.tooltip += "Currently has issues with multiple inspectors visible and erratic nudging position offset issues while scrolling.";
			alwaysUpdateShadersHelp.tooltip = "Forces the scene view to repaint every frame.  Huge performance cost, but will allow shaders based on time to update in realtime.";
			alwaysUpdateParticlesHelp.tooltip = "Forces the scene view to repaint every frame.  Huge performance cost, but will continually ensure particles are simulated.";
			particleUpdateRangeHelp.tooltip = "Range at which editor-updated particles will simulate.  Higher values will cost more performance.";
			if("Hooks (@Main)".ToLabel().DrawFoldout("Zios.Preferences.Hooks")){
				EditorGUI.indentLevel += 1;
				bool disableHooks = Utility.GetPref<bool>("EditorSettings-DisableHooks").Draw(disableHooksHelp);
				bool hideHooks = Utility.GetPref<bool>("EditorSettings-HideHooks").Draw(hideHooksHelp);
				bool temporaryHooks = Utility.GetPref<bool>("EditorSettings-TemporaryHooks").Draw(temporaryHooksHelp);
				if(GUI.changed){
					Utility.SetPref<bool>("EditorSettings-DisableHooks",disableHooks);
					Utility.SetPref<bool>("EditorSettings-HideHooks",hideHooks);
					Utility.SetPref<bool>("EditorSettings-TemporaryHooks",temporaryHooks);
				}
				EditorGUI.indentLevel -= 1;
			}
			if("Other".ToLabel().DrawFoldout("Zios.Preferences.Other")){
				EditorGUI.indentLevel += 1;
				bool fastInspector = Utility.GetPref<bool>("MonoBehaviourEditor-FastInspector").Draw(fastInspectorHelp);
				bool alwaysUpdateShaders = Utility.GetPref<bool>("EditorSettings-AlwaysUpdateShaders").Draw(alwaysUpdateShadersHelp);
				bool alwaysUpdateParticles = Utility.GetPref<bool>("EditorSettings-AlwaysUpdateParticles").Draw(alwaysUpdateParticlesHelp);
				float particleUpdateRange = Utility.GetPref<float>("EditorSettings-ParticleUpdateRange",100).Draw(particleUpdateRangeHelp);
				if(GUI.changed){
					Utility.SetPref<bool>("MonoBehaviourEditor-FastInspector",fastInspector);
					Utility.SetPref<bool>("EditorSettings-AlwaysUpdateShaders",alwaysUpdateShaders);
					Utility.SetPref<bool>("EditorSettings-AlwaysUpdateParticles",alwaysUpdateParticles);
					Utility.SetPref<float>("EditorSettings-ParticleUpdateRange",particleUpdateRange);
					Hook.SetState();
				}
				EditorGUI.indentLevel -= 1;
			}
			if(GUI.changed){
				Hook.SetState();
			}
		}
	}
}