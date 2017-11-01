using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Zios.Editors.AnimationEditors{
	using Animations;
	using Interface;
	using Event;
	using Config = Animations.AnimationConfiguration;
	[CustomEditor(typeof(AnimationSettings))]
	public class AnimationSettingsEditor : Editor{
		public static float lastTime;
		private List<AnimationConfiguration> active = new List<AnimationConfiguration>();
		public override void OnInspectorGUI(){
			EditorUI.Reset();
			EditorUI.allowIndention = false;
			Events.Add("On Editor Update",this.EditorUpdate);
			EditorGUILayout.BeginHorizontal();
			"Name".ToLabel().Layout(150).DrawLabel();
			"Rate •".ToLabel().Layout(50).DrawLabel();
			if(GUILayoutUtility.GetLastRect().Clicked()){
				Config.rateMode.Get().GetNames().DrawMenu(this.SetRateMode,Config.rateMode.Get().ToName().AsList());
			}
			"Speed •".ToLabel().Layout(50).DrawLabel();
			if(GUILayoutUtility.GetLastRect().Clicked()){
				Config.speedMode.Get().GetNames().DrawMenu(this.SetSpeedMode,Config.speedMode.Get().ToName().AsList());
			}
			"Blend".ToLabel().Layout(80).DrawLabel();
			"Wrap".ToLabel().Layout(115).DrawLabel();
			EditorGUILayout.EndHorizontal();
			foreach(var config in this.target.As<AnimationSettings>().animations){
				EditorGUILayout.BeginHorizontal();
				bool isPlaying = this.active.Contains(config);
				config.name.ToLabel().Layout(150).DrawLabel();
				config.rate = config.rate.Layout(50).Draw();
				config.speed = config.speed.Layout(50).Draw();
				config.blendMode = config.blendMode.Layout(80).Draw().As<AnimationBlendMode>();
				config.wrapMode = config.wrapMode.Layout(115).Draw().As<WrapMode>();
				if(isPlaying && "Stop".ToLabel().Layout(0,17).DrawButton()){this.Stop(config);}
				if(!isPlaying && "Play".ToLabel().Layout(0,17).DrawButton()){
					this.StopAll();
					this.active.AddNew(config);
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
		public void SetRateMode(object index){
			if(index.As<SpeedUnit>() == Config.rateMode){return;}
			Utility.RecordObject(this.target,"Animation Rate Mode Changed");
			Config.rateMode.Set(index.As<SpeedUnit>());
			foreach(var config in this.target.As<AnimationSettings>().animations){
				config.rate = Config.rateMode == SpeedUnit.Framerate ? config.rate*config.originalSpeed : config.rate/config.originalSpeed;
			}
			Utility.SetDirty(this.target);
		}
		public void SetSpeedMode(object index){
			if(index.As<SpeedUnit>() == Config.speedMode){return;}
			Utility.RecordObject(this.target,"Animation Speed Mode Changed");
			Config.speedMode.Set(index.As<SpeedUnit>());
			foreach(var config in this.target.As<AnimationSettings>().animations){
				config.speed = Config.speedMode == SpeedUnit.Framerate ? config.speed*config.originalSpeed : config.speed/config.originalSpeed;
			}
			Utility.SetDirty(this.target);
		}
		public void StopAll(){
			foreach(var active in this.active.Copy()){
				this.Stop(active);
			}
		}
		public void Stop(AnimationConfiguration config){
			config.time = 0;
			this.active.Remove(config);
			Events.Resume("On Hierarchy Changed");
			Utility.RepaintInspectors();
		}
		public void EditorUpdate(){
			var delta = Time.realtimeSinceStartup-AnimationSettingsEditor.lastTime;
			var weight = 1.0f/this.active.Count;
			foreach(var config in this.active.Copy()){
				if(config.IsNull() || config.name.IsEmpty()){continue;}
				Events.Pause("On Hierarchy Changed");
				var state = config.parent[config.name];
				state.weight = weight;
				config.parent.Blend(config.name,weight);
				config.time += delta*state.speed;
				var settings = this.target.As<AnimationSettings>();
				if(config.time >= state.clip.length){
					if(state.wrapMode == WrapMode.ClampForever){
						config.lastFrame = state.clip.length;
						state.clip.SampleAnimation(settings.gameObject,config.lastFrame.ToFloat());
						continue;
					}
					else if(state.wrapMode == WrapMode.Default || state.wrapMode == WrapMode.Once){
						this.Stop(config);
					}
				}
				var time = config.time%state.clip.length;
				var tick = 1.0d/state.clip.frameRate;
				time = time.ClampStep(tick);
				if(state.wrapMode == WrapMode.PingPong){
					if(config.time >= state.clip.length){
						time = state.clip.length-time;
						if(time <= 0.05f){config.time = 0;}
					}
				}
				if(time != config.lastFrame){
					config.lastFrame = time;
					state.clip.SampleAnimation(settings.gameObject,time.ToFloat());
				}
			}
			AnimationSettingsEditor.lastTime = Time.realtimeSinceStartup;
		}
	}
}