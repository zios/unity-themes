using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode]
[RequireComponent(typeof(Animation))]
[AddComponentMenu("Zios/Component/Animation/Animation Settings")]
public class AnimationSettings : MonoBehaviour{
	public List<AnimationConfig> configs = new List<AnimationConfig>();
	public void Start(){
		foreach(AnimationConfig config in this.configs){
			config.Apply();
		}
	}
	public void Update(){
		if(this.configs.Count == 0){
			foreach(AnimationState animation in this.animation){
				AnimationConfig config = new AnimationConfig(animation);
				this.configs.Add(config);
			}
		}
	}
}
[Serializable]
public class AnimationConfig{
	public string name;
	public float fps;
	public AnimationBlendMode blendMode;
	public WrapMode wrapMode;
	private AnimationState state;
	public AnimationConfig(AnimationState state){
		this.name = state.name;
		this.fps = state.clip.frameRate;
		this.blendMode = state.blendMode;
		this.wrapMode = state.clip.wrapMode;
		this.state = state;
	}
	public void Apply(){
		if(this.state != null){
			state.speed = this.fps / this.state.clip.frameRate;
			state.blendMode = this.blendMode;
			state.clip.wrapMode = this.wrapMode;
		}
	}
}
