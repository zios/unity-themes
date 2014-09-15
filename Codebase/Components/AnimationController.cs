using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
[Serializable]
public class AnimationData{
	public string name;
	public int priority = -1;
	public float holdDuration = -1;
	public AnimationData currentAnimation;
	[NonSerialized] public bool active;
	[NonSerialized] public float holdTime = -1;
	[NonSerialized] public float weight = 1;
	public AnimationData(string name,int priority=-1,float hold=-1){
		this.name = name;
		this.priority = priority;
		this.holdDuration = hold;
	}
}
[RequireComponent(typeof(Animation))][AddComponentMenu("Zios/Component/Animation/Animation Controller")]
public class AnimationController : MonoBehaviour{
	public bool highestPriorityOnly = true;
	public int defaultPriority = 1;
	public float transitionIn = 0.3f;
	public float transitionOut = 0.3f;
	public AnimationData defaultAnimation;
	public AnimationData currentAnimation;
	public List<AnimationData> animations = new List<AnimationData>();
	public Dictionary<string,AnimationData> current = new Dictionary<string,AnimationData>();
	public Dictionary<string,AnimationData> lookup = new Dictionary<string,AnimationData>();
	//=====================
	// Built-in
	//=====================
	public void OnValidate(){
		foreach(AnimationState state in this.animation){
			if(this.animations.Find(x=>x.name==state.name) == null){
				AnimationData data = new AnimationData(state.name,this.defaultPriority);
				this.animations.Add(data);
			}
		}
	}
	public void Awake(){
		foreach(AnimationData data in this.animations){
			this.lookup[data.name] = data;
		}
		Events.Add("SetAnimation",this.OnSet);
		Events.Add("SetAnimationSpeed",this.OnSetSpeed);
		Events.Add("SetAnimationWeight",this.OnSetWeight);
		Events.Add("SetAnimationHold",this.OnSetHold);
		Events.Add("PlayAnimation",this.OnPlay);
		Events.Add("StopAnimation",this.OnStop);
		Events.Add("HoldAnimation",this.OnHold);
		Events.Add("ReleaseAnimation",this.OnRelease);
	}
	public void Update(){
		foreach(AnimationData data in this.animations){
			if(this.current.ContainsKey(data.name)){
				AnimationData current = this.current[data.name];
				if(!current.active){
					if(Time.time > current.holdTime){current.holdTime = -1;}
					if(current.holdTime == -1){
						this.animation.Blend(data.name,0,this.transitionOut);
						this.current.Remove(data.name);
					}
				}
			}
		}
		if(this.current.Count == 0){
			this.Play(this.defaultAnimation);
		}
		this.currentAnimation = this.current.First().Value;
	}
	//=====================
	// Events
	//=====================
	public void OnHold(object[] values){
		string name =(string)values[0]; 
		int duration = (int)values[1];
		this.Hold(name,duration);
	}
	public void OnRelease(){this.Release();}
	public void OnStop(object[] values){
		if(values.Length > 0){
			string name = (string)values[0];
			this.Stop(name);	
		}
		else{
			this.Stop();
		}
	}
	public void OnPlay(object[] values){
		string name = (string)values[0];
		int priority = values.Length > 1 ? (int)values[1] : -1;
		this.Play(name,priority);
	}
	public void OnSet(object[] values){
		string name = (string)values[0];
		bool state = (bool)values[1];
		int priority = values.Length > 2 ? (int)values[2] : -1;
		this.Set(name,state,priority);
	}
	public void OnSetSpeed(object [] values){
		string name = (string)values[0];
		float speed = (float)values[1];
		this.SetSpeed(name,speed);
	}
	public void OnSetWeight(object [] values){
		string name = (string)values[0];
		float weight = (float)values[1];
		this.SetWeight(name,weight);
	}
	public void OnSetHold(object[] values){
		string name =(string)values[0]; 
		bool state = (bool)values[1];
		float duration = values.Length > 2 ? (float)values[1] : -1;
		this.SetHold(name,state,duration);
	}
	//=====================
	// Operations
	//=====================
	public void Play(string name,int priority=1){
		bool exists = this.lookup.ContainsKey(name);
		bool active = this.current.ContainsKey(name);
		if(exists && !active){
			if(this.highestPriorityOnly){
				if(priority == -1){priority = this.lookup[name].priority;}
				this.Stop(this.defaultAnimation.name);
				foreach(var item in this.current){
					if(item.Value.priority > priority){return;}
				}
			}
			
			this.current[name] = this.lookup[name]; 
			this.current[name].active = true;
			this.animation.Blend(name,1,this.transitionIn);
		}
	}
	public void Play(AnimationData data){
		this.Play(data.name,data.priority);
	}
	public void Stop(){
		foreach(var item in this.current){
			item.Value.active = false;
			item.Value.holdTime = Time.time + item.Value.holdDuration;
		}
	}
	public void Stop(string name){
		if(this.current.ContainsKey(name)){
			this.current[name].active = false;
		}
	}
	public void Stop(AnimationData data){this.Stop(data.name);}
	public void Hold(float duration){
		foreach(var item in this.current){
			item.Value.holdTime = Time.time + duration;
		}
	}
	public void Hold(string name,float duration){
		if(this.current.ContainsKey(name)){
			this.current[name].holdTime = Time.time + duration;
		}
	}
	public void Release(){
		foreach(var item in this.current){
			item.Value.holdTime = -1;
		}
	}
	public void Release(string name){
		if(this.current.ContainsKey(name)){
			this.current[name].holdTime = -1;
		}
	}
	public void Set(string name,bool state,int priority=1){
		if(state){
			this.Play(name,priority);
			return;
		}
		this.Stop(name);
	}
	public void SetSpeed(string name,float speed){
		if(this.animation[name]){
			this.animation[name].speed = speed;
		}
	}
	public void SetWeight(string name,float weight){
		if(this.animation[name]){
			this.animation[name].weight = weight;
		}
	}
	public void SetHold(string name,bool state,float duration){
		if(state){
			this.Hold(name,duration);
			return;
		}
		this.Release(name);
	}
}