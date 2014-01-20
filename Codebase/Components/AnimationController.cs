using UnityEngine;
using System;
[RequireComponent(typeof(Animation))]
[AddComponentMenu("Zios/Component/Animation/Animation Controller")]
public class AnimationController : MonoBehaviour{
	public string defaultAnimation = "Idle";
	public string currentAnimation;
	[NonSerialized] public int currentPriority;
	public void Awake(){
		Events.Add("SetAnimation",this.OnSet);
		Events.Add("PlayAnimation",this.OnPlay);
		Events.Add("StopAnimation",this.OnStop);
	}
	public void Update(){
		if(!this.animation.isPlaying){
			this.currentPriority = 0;
			this.Play(this.defaultAnimation,0);
		}
	}
	public void OnStop(string name){this.Stop(name);}
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
	public void Play(string name,int priority=1){
		bool exists = this.animation[name]; 
		if(exists && priority >= this.currentPriority){
			this.animation.Play(name);
			this.currentAnimation = name;
			this.currentPriority = priority;
		}
	}
	public void Stop(string name){
		bool exists = this.animation[name]; 
		if(exists){
			this.animation.Stop(name);
		}
	}
	public void Set(string name,bool state,int priority=1){
		if(state){
			this.Play(name,priority);
		}
		else{
			this.Stop(name);
		}
	}
}
