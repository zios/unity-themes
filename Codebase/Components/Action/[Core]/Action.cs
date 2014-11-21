#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/*/Action")]
	public class Action : StateMonoBehaviour{
		[NonSerialized] public StateController controller;
		[NonSerialized] public GameObject owner;
		public override void Awake(){
			this.alias = this.gameObject.name;
			if(this.owner.IsNull()){
				this.controller = this.gameObject.GetComponentInParent<StateController>(true);
				this.owner = this.controller == null ? this.gameObject : this.controller.gameObject;
			}
			if(!this.controller.IsNull()){
				this.inUse.AddScope(this.controller);
				this.usable.AddScope(this.controller);
			}
			Events.Add("Action End (Force)",this.End,this.owner);
			Events.Register("@Update Parts",this.gameObject);
			Events.Register("Action Start",this.gameObject);
			Events.Register("Action End",this.gameObject);
			Events.Register("Action Disabled",this.gameObject);
			if(this.owner != this.gameObject){
				Events.Register("@Update States",this.owner);
				Events.Register("@Refresh",this.owner);
				Events.Register(this.alias+"/Start",this.owner);
				Events.Register(this.alias+"/End",this.owner);
			}
			base.Awake();
			this.usable.Set(this.controller==null);
		}
		public virtual void FixedUpdate(){
			if(!Application.isPlaying){return;}
			if(this.usable && this.ready){this.Use();}
			else if(!this.usable){this.End();}
		}
		public void OnDestroy(){
			if(!this.owner.IsNull()){
				this.owner.Call("@Refresh");
			}
		}
		public void OnDisable(){
			this.gameObject.Call("Action Disabled");
			this.gameObject.Call("@Update Parts");
		}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "Start" : "End";
				this.inUse.Set(state);
				this.gameObject.Call("Action "+active);
				this.owner.Call(this.alias+" "+active);
				this.owner.Call("@Update States");
				this.gameObject.Call("@Update Parts");
			}
		}
	}
	public enum ActionRate{Default,FixedUpdate,Update,LateUpdate,None};
	public enum ActionOccurrence{Default,Constant,Once};
	[AddComponentMenu("")]
	public class ActionPart : StateMonoBehaviour{
		public static Dictionary<GameObject,bool> dirty = new Dictionary<GameObject,bool>();
		public static Dictionary<ActionPart,List<ActionPart>> once = new Dictionary<ActionPart,List<ActionPart>>();
		public static ActionPart current;
		public ActionRate rate = ActionRate.Default;
		public ActionOccurrence occurrence = ActionOccurrence.Constant;
		[NonSerialized] public Action action;
		[HideInInspector] public int priority = -1;
		[HideInInspector] public bool hasReset;
		private bool requirableOverride;
		public override void Reset(){
			this.hasReset = true;
			this.Awake();
		}
		public override void Awake(){
			base.Awake();
			if(this.alias.IsEmpty()){
				this.alias = this.GetType().ToString();
			}
			if(action.IsNull()){
				this.action = this.GetComponent<Action>(true);
				if(!action.IsNull()){
					this.action.Awake();
				}
			}
			Events.Add(this.alias+"/End",this.End);
			Events.Add(this.alias+"/Start",this.Use);
			Events.Add(this.alias+"/End (Force)",this.ForceEnd);
			Events.Register("@Refresh",this.gameObject);
			Events.Register("@Update Parts",this.gameObject);
			Events.Register(this.alias+"/Disabled",this.gameObject);
			Events.Register(this.alias+"/Started",this.gameObject);
			Events.Register(this.alias+"/Ended",this.gameObject);
			bool controlled = this.action != null && this.action.controller != null;
			this.usable.Set(!controlled);
			this.SetDirty(true);
			if(Application.isPlaying){
				ActionPart.once[this] = new List<ActionPart>();
			}
		}
		[ContextMenu("Toggle Column Visibility")]
		public void ToggleRequire(){
			this.requirable = !this.requirable;
			this.requirableOverride = !this.requirableOverride;
			this.gameObject.Call("@Refresh");
		}
		public virtual void Step(){
			if(!Application.isPlaying){return;}
			ActionPart.current = this;
			if(ActionPart.once[this].Count > 0){
				foreach(ActionPart delay in ActionPart.once[this]){
					delay.inUse.Set(false);
					delay.SetDirty(true);
				}
				ActionPart.once[this].Clear();
			}
			if(ActionPart.dirty[this.gameObject]){
				this.gameObject.Call("@Update Parts");
				this.SetDirty(false);
			}
			bool partHappened = this.used && this.occurrence == ActionOccurrence.Once;
			bool actionUsable = this.action == null || this.action.usable;
			if(!partHappened){
				if(actionUsable && this.usable){this.Use();}
				else if(this.inUse){
					this.End();
				}
			}
		}
		public virtual void FixedUpdate(){
			if(this.rate == ActionRate.FixedUpdate){
				this.Step();
			}
		}
		public virtual void Update(){
			if(this.rate == ActionRate.Update || this.rate == ActionRate.Default){
				this.Step();
			}
		}
		public virtual void LateUpdate(){
			if(this.rate == ActionRate.LateUpdate){
				this.Step();
			}
		}
		public void OnDestroy(){
			if(!this.gameObject.IsNull()){
				this.gameObject.Call("@Refresh");
			}
		}
		public void OnDisable(){
			if(!this.gameObject.activeSelf){
				this.gameObject.Call(this.alias+"/Disabled");
				this.End();
			}
		}
		public void DefaultRate(string rate){
			if(this.rate == ActionRate.Default){
				if(rate == "FixedUpdate"){this.rate = ActionRate.FixedUpdate;}
				if(rate == "LateUpdate"){this.rate = ActionRate.LateUpdate;}
			}
		}
		public void DefaultOccurrence(string occurrence){
			if(this.occurrence == ActionOccurrence.Default){
				if(occurrence == "Constant"){this.occurrence = ActionOccurrence.Constant;}
				if(occurrence == "Once"){this.occurrence = ActionOccurrence.Once;}
				//if(occurrence == "Never"){this.occurrence = ActionOccurrence.Never;}
			}
		}
		public void DefaultPriority(int priority){
			if(this.priority == -1){
				this.priority = priority;
			}
		}
		public void DefaultAlias(string name){
			if(string.IsNullOrEmpty(this.alias)){
				this.stateAlias = name;
				this.alias = name;
			}
		}
		public void DefaultRequirable(bool state){
			if(!this.requirableOverride){
				this.requirable = state;
			}
		}
		public virtual void ForceEnd(){this.Toggle(false,true);}
		public virtual void SetDirty(bool state){ActionPart.dirty[this.gameObject] = state;}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public void Toggle(bool state,bool force=false){
			bool onceReset = this.used && this.occurrence == ActionOccurrence.Once;
			if(onceReset || state != this.inUse || force){
				string active = state ? "/Started" : "/Ended";
				this.used = state;
				this.inUse.Set(state);
				this.gameObject.Call(this.alias+active);
				if(state && this.occurrence == ActionOccurrence.Once){
					if(!ActionPart.once[ActionPart.current].Contains(this)){
						ActionPart.once[ActionPart.current].Add(this);
					}
				}
				this.SetDirty(true);
			}
		}
	}
}
