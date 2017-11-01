using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Animations{
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
		public static Store<SpeedUnit> rateMode = new Store<SpeedUnit>("AnimationSettings.rateMode");
		public static Store<SpeedUnit> speedMode = new Store<SpeedUnit>("AnimationSettings.speedMode",SpeedUnit.Scalar);
		public string name;
		public float rate;
		public float speed;
		public float originalSpeed;
		[NonSerialized] public double time = 0;
		[NonSerialized] public double lastFrame = 0;
		public AnimationBlendMode blendMode;
		public WrapMode wrapMode;
		public Animation parent;
		private AnimationState animationState;
		public static AnimationConfiguration Create(AnimationState state){
			var config = new AnimationConfiguration();
			config.name = state.name;
			config.rate = AnimationConfiguration.rateMode == SpeedUnit.Scalar ? 1 : state.clip.frameRate;
			config.speed = AnimationConfiguration.speedMode == SpeedUnit.Scalar ? state.speed : state.speed*state.clip.frameRate;
			config.originalSpeed = state.clip.frameRate;
			config.blendMode = state.blendMode;
			config.wrapMode = state.clip.wrapMode;
			return config;
		}
		public void Apply(){
			var state = this.parent[this.name];
			if(state != null && state.clip != null){
				state.clip.frameRate = AnimationConfiguration.rateMode == SpeedUnit.Scalar ? this.rate*this.originalSpeed : this.rate;
				state.speed = AnimationConfiguration.speedMode == SpeedUnit.Scalar ? this.speed : this.speed/this.originalSpeed;
				state.blendMode = this.blendMode;
				state.wrapMode = this.wrapMode;
				state.clip.wrapMode = this.wrapMode;
			}
		}
	}
	public enum SpeedUnit{Framerate,Scalar};
}