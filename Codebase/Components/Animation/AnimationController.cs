using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Animations{
	using Attributes;
	using Event;
	[Serializable]
	public class AnimationData{
		public string name;
		public int priority = -1;
		public float weight = -1;
		public float transitionIn = -1;
		public float transitionOut = -1;
		public bool rewindOnPlay = true;
		public AnimationData currentAnimation;
		public AnimationState state;
		[NonSerialized] public float originalWeight;
		[NonSerialized] public bool active;
	}
	[RequireComponent(typeof(Animation))][AddComponentMenu("Zios/Component/Animation/Animation Controller")]
	public class AnimationController : ManagedMonoBehaviour{
		public AttributeBool highestPriorityOnly = true;
		public AttributeInt defaultPriority = 1;
		public AttributeFloat defaultTransitionIn = 0.15f;
		public AttributeFloat defaultTransitionOut = 0.15f;
		public AttributeString defaultAnimationName = "";
		[Internal][ReadOnly] public AnimationData defaultAnimation;
		[Internal][ReadOnly] public List<AnimationData> currentAnimations;
		public List<AnimationData> animations = new List<AnimationData>();
		public Dictionary<string,AnimationData> current = new Dictionary<string,AnimationData>();
		public Dictionary<string,AnimationData> lookup = new Dictionary<string,AnimationData>();
		//=====================
		// Built-in
		//=====================
		public override void Awake(){
			base.Awake();
			this.Build();
			this.highestPriorityOnly.Setup("Highest Priority Only",this);
			this.defaultPriority.Setup("Default Priority Only",this);
			this.defaultTransitionIn.Setup("Default Transition In",this);
			this.defaultTransitionOut.Setup("Default Transition Out",this);
			this.defaultAnimationName.Setup("Default Animation Name",this);
			Events.Add("Set Animation",this.OnSet,this.gameObject);
			Events.Add("Set Animation Default",this.OnSetDefault,this.gameObject);
			Events.Add("Set Animation Speed",this.OnSetSpeed,this.gameObject);
			Events.Add("Set Animation Weight",this.OnSetWeight,this.gameObject);
			Events.Add("Play Animation",this.OnPlay,this.gameObject);
			Events.Add("Stop Animation",this.OnStop,this.gameObject);
			if(this.animations.Count > 0){
				this.defaultAnimationName.SetDefault(this.animations.First().name);
			}
		}
		public override void Start(){
			base.Start();
			if(Application.isPlaying){
				foreach(AnimationData data in this.animations){
					this.lookup[data.name] = data;
					if(data.weight == -1){data.weight = 1.0f;}
					if(data.priority == -1){data.priority = this.defaultPriority;}
					if(data.transitionIn == -1){data.transitionIn = this.defaultTransitionIn;}
					if(data.transitionOut == -1){data.transitionOut = this.defaultTransitionOut;}
					data.originalWeight = data.weight;
				}
				if(this.lookup.ContainsKey(this.defaultAnimationName)){
					this.defaultAnimation = this.lookup[this.defaultAnimationName];
					this.defaultAnimation.active = true;
					this.current[this.name] = this.defaultAnimation;
				}
				else{
					Debug.LogWarning("[AnimationController] Default animation (" + this.defaultAnimationName + ") not found.",this.gameObject);
				}
			}
		}
		private void PlayDefault(){
			bool exists = this.defaultAnimation != null && this.lookup.ContainsKey(this.defaultAnimation.name);
			if(!exists){return;}
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
			this.GetComponent<Animation>().Blend(name,currentWeight,transitionTime);
		}
		public override void Step(){
			this.PlayDefault();
			foreach(AnimationData data in this.animations){
				if(this.current.ContainsKey(data.name)){
					string name = data.name;
					bool zeroWeight = this.lookup[name].weight <= 0.01f && data != this.defaultAnimation;
					if(!data.active || zeroWeight){
						this.GetComponent<Animation>().Blend(name,0,data.transitionOut);
						data.weight = data.originalWeight;
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
		private void Build(){
			Animation animations = this.GetComponent<Animation>();
			foreach(AnimationState state in animations){
				AnimationData data = this.animations.Find(x=>x.name==state.name);
				if(data == null){
					data = new AnimationData();
					this.animations.Add(data);
				}
				data.name = state.name;
				data.state = state;
			}
		}
		[ContextMenu("Restore Defaults")]
		private void RestoreDefaults(){
			foreach(AnimationState state in this.GetComponent<Animation>()){
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
				this.defaultAnimationName.Set(name);
				this.defaultAnimation = this.lookup[name];
			}
		}
		public void OnSetSpeed(object[] values){
			string name = (string)values[0];
			float speed = (float)values[1];
			this.SetSpeed(name,speed);
		}
		public void OnSetWeight(object[] values){
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
				if(this.current[name].rewindOnPlay){this.GetComponent<Animation>().Rewind(name);}
				this.GetComponent<Animation>().Blend(name,this.lookup[name].weight,this.lookup[name].transitionIn);
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
			if(this.GetComponent<Animation>()[name]){
				this.GetComponent<Animation>()[name].speed = speed;
			}
		}
		public void SetWeight(string name,float weight){
			if(this.GetComponent<Animation>()[name] && weight != this.lookup[name].weight){
				AnimationData data = this.lookup[name];
				data.weight = weight;
				if(current.ContainsKey(name)){
					float transitionTime = data.state.weight < weight ? data.transitionOut : data.transitionOut;
					this.GetComponent<Animation>().Blend(name,data.weight,transitionTime);
				}
			}
		}
	}
}