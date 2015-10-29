using UnityEngine;
using UnityEditor;
namespace Zios.UI{
    [CustomEditor(typeof(AnimationSettings))]
    public class AnimationSettingsEditor : Editor{
		public AnimationConfiguration active;
		public float time = 0;
	    public override void OnInspectorGUI(){
			var labelStyle = EditorStyles.label.FixedWidth(120);
			var fpsStyle = EditorStyles.numberField.FixedWidth(50);
			var blendStyle = EditorStyles.popup.FixedWidth(70);
			var wrapStyle = EditorStyles.popup.FixedWidth(100);
			EditorGUILayout.BeginHorizontal();
			"Name".DrawLabel(labelStyle);
			"FPS".DrawLabel(labelStyle.FixedWidth(30));
			"Blend".DrawLabel(labelStyle.FixedWidth(80));
			"Wrap".DrawLabel(labelStyle.FixedWidth(100));

			EditorGUILayout.EndHorizontal();
			foreach(var config in this.target.As<AnimationSettings>().animations){
				EditorGUILayout.BeginHorizontal();
				bool isPlaying = config == active;
				config.name.DrawLabel(labelStyle);
				config.fps.Draw(null,fpsStyle);
				config.blendMode = (AnimationBlendMode)config.blendMode.Draw("",blendStyle);
				config.wrapMode = (WrapMode)config.wrapMode.Draw("",wrapStyle);
				if(isPlaying && "Stop".DrawButton()){
				    this.active = null;
					Events.Resume("On Hierarchy Changed");
				}
				if(!isPlaying && "Play".DrawButton()){
				    this.time = 0;
				    this.active = config;
					Events.Pause("On Hierarchy Changed");
				}
				if(GUI.changed){
					config.Apply();
					Utility.SetDirty(this.target);
				}
				EditorGUILayout.EndHorizontal();
			}
	    }
		public void EditorUpdate(){
			if(this.active != null){
				var state = this.active.parent[this.active.name];
				var clip = state.clip;
				bool loop = clip.wrapMode == WrapMode.Loop;
				float animationTime = clip.length * (clip.frameRate);
				float framerate = 10000.0f / (clip.frameRate * state.speed);
				this.time += (animationTime / framerate);
				if(this.time >= animationTime){
					this.time = loop ? 0 : animationTime;
				}
				var settings = this.target.As<AnimationSettings>();
				clip.SampleAnimation(settings.gameObject,Time.realtimeSinceStartup);
			}
		}
    }
}