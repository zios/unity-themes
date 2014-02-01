using UnityEngine;
using System;
[RequireComponent(typeof(Animation))]
[AddComponentMenu("Zios/Component/Animation/Animation Controller")]
public class AnimationController : MonoBehaviour{
	public string defaultAnimation = "Idle";
	public string currentAnimation;
	[NonSerialized] public float animationHoldEndTime = -1;
	[NonSerialized] public int currentPriority;
	//=====================
	// Built-in
	//=====================
	public void Awake(){
		Events.Add("SetAnimation",this.OnSet);
		Events.Add("SetAnimationHold",this.OnSetHold);
		Events.Add("PlayAnimation",this.OnPlay);
		Events.Add("StopAnimation",this.OnStop);
		Events.Add("HoldAnimation",this.OnHold);
		Events.Add("ReleaseAnimation",this.OnRelease);
	}
	public void Update(){
		if((Time.time > this.animationHoldEndTime) && (this.animationHoldEndTime != -1)){
			this.Stop(this.currentAnimation);
		}
		if(!this.animation.isPlaying){
			this.currentPriority = 0;
			this.Play(this.defaultAnimation,0);
		}
	}
	//=====================
	// Events
	//=====================
	public void OnHold(float duration){this.Hold(duration);}
	public void OnRelease(){this.Release();}
	public void OnStop(object[] values){
		string name = values.Length > 0 ? (string)values[0] : ""; 
		if(name == ""){this.Stop();}
		else{this.Stop(name);}
	}
	public void OnPlay(object[] values){
		string name = (string)values[0];
		int priority = values.Length > 1 ? (int)values[1] : 1;
		this.Play(name,priority);
	}
	public void OnSet(object[] values){
		string name = (string)values[0];
		bool state = (bool)values[1];
		int priority = values.Length > 2 ? (int)values[2] : 1;
		this.Set(name,state,priority);
	}
	public void OnSetHold(object[] values){
		float duration = (float)values[0];
		bool state = (bool)values[1];
		string name = values.Length > 2 ? (string)values[2] : "";
		this.SetHold(duration,state,name);
	}
	//=====================
	// Operations
	//=====================
	public void Play(string name,int priority=1){
		bool exists = this.animation[name];
		bool notCurrent = (name != this.currentAnimation);
		bool isPlaying = this.animation.IsPlaying(name);
		if(!isPlaying && exists && notCurrent && priority >= this.currentPriority){
			this.animation.Play(name);
			this.currentAnimation = name;
			this.currentPriority = priority;
			this.animationHoldEndTime = -1;
		}
	}
	public void Stop(){
		if(Time.time > this.animationHoldEndTime){
			this.animationHoldEndTime = -1;
			this.currentAnimation = "";
			this.currentPriority = 0;
			this.animation.Stop();
		}
	}
	public void Stop(string name){
		bool isPlaying = this.animation.IsPlaying(name);
		if(isPlaying && Time.time > this.animationHoldEndTime){
			if(name == this.currentAnimation){
				this.animationHoldEndTime = -1;
				this.currentAnimation = "";
				this.currentPriority = 0;
			}
			this.animation.Stop(name);
		}
	}
	public void Hold(float duration){
		if(duration > 0){
			this.animationHoldEndTime = Time.time + duration;
		}
	}
	public void Release(string name=""){
		if(name == ""){name = this.currentAnimation;}
		if(name == this.currentAnimation){
			this.animationHoldEndTime = -1;
		}
	}
	public void Set(string name,bool state,int priority=1){
		if(state){
			this.Play(name,priority);
			return;
		}
		this.Stop(name);
	}
	public void SetHold(float duration,bool state,string name=""){
		if(state){
			this.Hold(duration);
			return;
		}
		this.Release(name);
	}
}