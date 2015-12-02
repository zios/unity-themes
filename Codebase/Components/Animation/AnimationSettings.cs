using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	[ExecuteInEditMode][AddComponentMenu("Zios/Component/Animation/Animation Settings")]
	public class AnimationSettings : MonoBehaviour{
		public List<AnimationConfiguration> animations = new List<AnimationConfiguration>();
		public void Reset(){
			this.animations = new List<AnimationConfiguration>();
			this.Start();
		}
		public void Start(){
			this.Build();
			foreach(var config in this.animations){
				config.Apply();
			}
		}
		public void Build(){
			var animation = gameObject.GetComponent<Animation>();
			if(this.animations.Count == 0 && !animation.IsNull()){
				foreach(AnimationState state in animation){
					var config = AnimationConfiguration.Create(state);
					this.animations.Add(config);
				}
			}
			foreach(var config in this.animations){config.parent = animation;}
		}
	}
	[Serializable]
	public class AnimationConfiguration{
		public string name;
		public float fps;
		public AnimationBlendMode blendMode;
		public WrapMode wrapMode;
		public Animation parent;
		private AnimationState animationState;
		public static AnimationConfiguration Create(AnimationState state){
			var config = new AnimationConfiguration();
			config.name = state.name;
			config.fps = state.clip.frameRate;
			config.blendMode = state.blendMode;
			config.wrapMode = state.clip.wrapMode;
			return config;
		}
		public void Apply(){
			var state = this.parent[this.name];
			if(state != null && state.clip != null){
				state.speed = this.fps / state.clip.frameRate;
				state.blendMode = this.blendMode;
				state.clip.wrapMode = this.wrapMode;
			}
		}
	}
}