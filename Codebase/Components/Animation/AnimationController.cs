using UnityEngine;
using Zios;
using System;
using System.Linq;
using System.Collections.Generic;
[Serializable]
public class AnimationData{
	public string name;
	public int priority = -1;
	public float weight = -1;
	public float transitionIn = -1;
	public float transitionOut = -1;
	public AnimationData currentAnimation;
	public AnimationState state;
	[NonSerialized] public bool active;
}
[RequireComponent(typeof(Animation))][AddComponentMenu("Zios/Component/Animation/Animation Controller")]
public class AnimationController : MonoBehaviour{
	public AttributeBool highestPriorityOnly = true;
	public AttributeInt defaultPriority = 1;
	public AttributeFloat defaultTransitionIn = 0.15f;
	public AttributeFloat defaultTransitionOut = 0.15f;
	public AttributeString defaultAnimationName;
	[ReadOnly] public AnimationData defaultAnimation;
	[ReadOnly] public List<AnimationData> currentAnimations;
	public List<AnimationData> animations = new List<AnimationData>();
	public Dictionary<string,AnimationData> current = new Dictionary<string,AnimationData>();
	public Dictionary<string,AnimationData> lookup = new Dictionary<string,AnimationData>();
	//=====================
	// Built-in
	//=====================
	public void OnValidate(){
		//if(this.animation == null){return;}
		this.highestPriorityOnly.Setup("Highest Priority Only",this);
		this.defaultPriority.Setup("Default Priority Only",this);
		this.defaultTransitionIn.Setup("Default Transition In",this);
		this.defaultTransitionOut.Setup("Default Transition Out",this);
		this.defaultAnimationName.Setup("Default Animation Name",this);
		foreach(AnimationState state in this.animation){
			AnimationData data = this.animations.Find(x=>x.name==state.name);
			if(data == null){
				data = new AnimationData();
				this.animations.Add(data);
			}
			data.name = state.name;
			data.state = state;
		}
	}
	public void Awake(){
		foreach(AnimationData data in this.animations){
			this.lookup[data.name] = data;
			if(data.weight == -1){data.weight = 1.0f;}
			if(data.priority == -1){data.priority = this.defaultPriority;}
			if(data.transitionIn == -1){data.transitionIn = this.defaultTransitionIn;}
			if(data.transitionOut == -1){data.transitionOut = this.defaultTransitionOut;}
		}
		if(this.lookup.ContainsKey(this.defaultAnimationName)){
			this.defaultAnimation = this.lookup[this.defaultAnimationName];
			this.defaultAnimation.active = true;
			this.current[name] = this.defaultAnimation;
		}
		else{
			Debug.LogWarning("AnimationController : Default animation (" + this.defaultAnimationName + ") not found.");
		}
		Events.Add("Set Animation",this.OnSet);
		Events.Add("Set Animation Default",this.OnSetDefault);
		Events.Add("Set Animation Speed",this.OnSetSpeed);
		Events.Add("Set Animation Weight",this.OnSetWeight);
		Events.Add("Play Animation",this.OnPlay);
		Events.Add("Stop Animation",this.OnStop);
	}
	private void PlayDefault(){
		if(this.defaultAnimation == null){return;}
		float currentWeight = 0;
		AnimationData fallback = this.defaultAnimation;
		string name = this.defaultAnimation.name;
		foreach(var item in this.current){
			if(item.Value == fallback){continue;}
			currentWeight += item.Value.state.weight;
		}
		currentWeight = currentWeight < 1 ? 1.0f-currentWeight : 0;
		float transitionTime = this.lookup[name].weight < currentWeight ? fallback.transitionOut : fallback.transitionOut;
		this.lookup[name].weight = currentWeight;
		this.animation.Blend(name,currentWeight,transitionTime);
	}
	public void Update(){
		this.PlayDefault();
		foreach(AnimationData data in this.animations){
			if(this.current.ContainsKey(data.name)){
				string name = data.name;
				AnimationData current = this.current[name];
				bool zeroWeight = this.lookup[name].weight <= 0.01f && current != this.defaultAnimation;
				if(!current.active || zeroWeight){
					this.animation.Blend(name,0,data.transitionOut);
					current.weight = 1;
					this.current.Remove(name);
				}
			}
		}
		if(Application.isEditor){
			this.currentAnimations = this.current.Values.ToList();
		}
	}
	//=====================
	// Internal
	//=====================
	[ContextMenu("Restore Defaults")]
	private void RestoreDefaults(){
		foreach(AnimationState state in this.animation){
			AnimationData data = this.animations.Find(x=>x.name==state.name);
			if(data != null){
				data.weight = -1;
				data.transitionOut = -1;
				data.transitionIn = -1;
				data.priority = -1;
			}
		}
	}
	//=====================
	// Events
	//=====================
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
	public void OnSetDefault(object[] values){
		string name = (string)values[0];
		if(this.lookup.ContainsKey(name)){
			this.defaultAnimationName = name;
			this.defaultAnimation = this.lookup[name];
		}
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
	//=====================
	// Operations
	//=====================
	public void Play(string name,int priority=1){
		bool exists = this.lookup.ContainsKey(name);
		bool active = this.current.ContainsKey(name);
		if(exists && !active){
			if(this.highestPriorityOnly){
				if(priority == -1){priority = this.lookup[name].priority;}
				foreach(var item in this.current){
					if(item.Value.priority > priority){return;}
				}
				foreach(var item in this.current){
					if(priority > item.Value.priority){this.Stop(item.Value.name);}
				}
			}
			this.current[name] = this.lookup[name]; 
			this.current[name].active = true;
			this.animation.Rewind(name);
			this.animation.Blend(name,this.lookup[name].weight,this.lookup[name].transitionIn);
		}
	}
	public void Play(AnimationData data){
		this.Play(data.name,data.priority);
	}
	public void Stop(){
		foreach(var item in this.current){
			item.Value.active = false;
		}
	}
	public void Stop(string name){
		if(this.current.ContainsKey(name)){
			this.current[name].active = false;
		}
	}
	public void Stop(AnimationData data){this.Stop(data.name);}
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
		if(this.animation[name] && weight != this.lookup[name].weight){
			AnimationData data = this.lookup[name];
			data.weight = weight;
			if(current.ContainsKey(name)){
				float transitionTime = data.state.weight < weight ? data.transitionOut : data.transitionOut;
				this.animation.Blend(name,data.weight,transitionTime);
			}
		}
	}
}