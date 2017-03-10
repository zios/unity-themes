using UnityEngine;
using UnityEditor;
namespace Zios.Editors.AnimationEditors{
	using Animations;
	using Interface;
	using Event;
	[CustomEditor(typeof(AnimationSettings))]
	public class AnimationSettingsEditor : Editor{
		public static AnimationSettingsEditor instance;
		public AnimationConfiguration active;
		public float time = 0;
		public override void OnInspectorGUI(){
			EditorUI.Reset();
			EditorUI.allowIndention = false;
			AnimationSettingsEditor.instance = this;
			Events.Add("On Editor Update",AnimationSettingsEditor.EditorUpdate);
			EditorGUILayout.BeginHorizontal();
			"Name".ToLabel().Layout(120).DrawLabel();
			"FPS".ToLabel().Layout(30).DrawLabel();
			"Blend".ToLabel().Layout(80).DrawLabel();
			"Wrap".ToLabel().Layout(100).DrawLabel();
			EditorGUILayout.EndHorizontal();
			foreach(var config in this.target.As<AnimationSettings>().animations){
				EditorGUILayout.BeginHorizontal();
				bool isPlaying = config == active;
				config.name.ToLabel().Layout(120).DrawLabel();
				config.fps = config.fps.Layout(30).Draw();
				config.blendMode = config.blendMode.Layout(80).Draw().As<AnimationBlendMode>();
				config.wrapMode = config.wrapMode.Layout(100).Draw().As<WrapMode>();
				if(isPlaying && "Stop".ToLabel().Layout(0,17).DrawButton()){this.Stop();}
				if(!isPlaying && "Play".ToLabel().Layout(0,17).DrawButton()){
					this.time = 0;
					this.active = config;
					Events.Pause("On Hierarchy Changed");
				}
				if(GUI.changed){
					Utility.RecordObject(this.target,"Animation Settings Changed");
					config.Apply();
					Utility.SetDirty(this.target);
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		public void Stop(){
			this.active = null;
			Events.Resume("On Hierarchy Changed");
		}
		public static void EditorUpdate(){
			var instance = AnimationSettingsEditor.instance;
			if(!instance.IsNull() && !instance.active.IsNull() && !instance.active.name.IsEmpty()){
				Events.Pause("On Hierarchy Changed");
				var state = instance.active.parent[instance.active.name];
				instance.time += (state.clip.frameRate * state.speed) / (10000*0.4f);
				var settings = instance.target.As<AnimationSettings>();
				if(state.wrapMode != WrapMode.Loop && instance.time >= state.clip.length){
					instance.Stop();
					Utility.RepaintInspectors();
				}
				state.clip.SampleAnimation(settings.gameObject,instance.time%state.clip.length);
			}
		}
	}
}