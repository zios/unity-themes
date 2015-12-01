using UnityEngine;
using UnityEditor;
namespace Zios.UI{
	[CustomEditor(typeof(AnimationSettings))]
	public class AnimationSettingsEditor : Editor{
		public static AnimationSettingsEditor instance;
		public AnimationConfiguration active;
		public float time = 0;
		public override void OnInspectorGUI(){
			AnimationSettingsEditor.instance = this;
			Events.Add("On Editor Update",this.EditorUpdate);
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
				config.fps = config.fps.Draw(null,fpsStyle);
				config.blendMode = (AnimationBlendMode)config.blendMode.Draw("",blendStyle);
				config.wrapMode = (WrapMode)config.wrapMode.Draw("",wrapStyle);
				if(isPlaying && "Stop".DrawButton()){this.Stop();}
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