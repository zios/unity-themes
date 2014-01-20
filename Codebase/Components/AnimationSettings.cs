using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
[AddComponentMenu("Zios/Component/Animation/Animation Settings")]
[ExecuteInEditMode]
public class AnimationSettings : MonoBehaviour{
	public List<AnimationConfiguration> animations = new List<AnimationConfiguration>();
	public void Start(){
		if(gameObject.animation != null){
			foreach(AnimationConfiguration configuration in this.animations){
				configuration.Apply(gameObject.animation);
			}
		}
	}
	public void Update(){
		if(this.animations.Count == 0 && gameObject.animation != null){
			foreach(AnimationState animationState in gameObject.animation){
				this.animations.Add(AnimationConfiguration.FromAnimation(animationState));
			}
		}
	}
}
[Serializable]
public class AnimationConfiguration{
	public string name;
	public float fps;
	public AnimationBlendMode blendMode;
	public WrapMode wrapMode;
	private AnimationState animationState;
	public static AnimationConfiguration FromAnimation(AnimationState animationState){
		AnimationConfiguration configuration = new AnimationConfiguration();
		configuration.name = animationState.name;
		configuration.fps = animationState.clip.frameRate;
		configuration.blendMode = animationState.blendMode;
		configuration.wrapMode = animationState.clip.wrapMode;
		return configuration;
	}
	public void Apply(Animation animation){
		AnimationState animationState = animation[this.name];
		if(animationState != null){
			animationState.speed = this.fps / animationState.clip.frameRate;
			animationState.blendMode = this.blendMode;
			animationState.clip.wrapMode = this.wrapMode;
		}
	}
}
