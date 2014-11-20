#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/*/Action")]
	public class Action : StateMonoBehaviour{
		public static Dictionary<GameObject,bool> dirty = new Dictionary<GameObject,bool>();
		[NonSerialized] public StateController controller;
		[NonSerialized] public GameObject owner;
		public override void Awake(){
			if(this.owner.IsNull()){
				this.controller = this.gameObject.GetComponentInParent<StateController>(true);
				this.owner = this.controller == null ? this.gameObject : this.controller.gameObject;
			}
			this.alias = this.gameObject.name;
			this.inUse.Setup("Active",this);
			this.usable.Setup("Usable",this);
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
			this.usable.Set(this.controller==null);
			this.SetDirty(true);
		}
		public virtual void FixedUpdate(){
			if(!Application.isPlaying){return;}
			if(Action.dirty.ContainsKey(this.gameObject) && Action.dirty[this.gameObject]){
				this.SetDirty(false);
				Action.dirty[this.gameObject] = false;
				this.gameObject.Call("@Update Parts");
			}
			//Debug.Log(this.alias + " -- " + this.usable.Get() + " -- " + this.ready.Get());
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
		public void SetDirty(bool state){Action.dirty[this.gameObject] = state;}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "Start" : "End";
				this.inUse.Set(state);
				this.gameObject.Call("Action "+active);
				this.owner.Call(this.alias+" "+active);
				this.owner.Call("@Update States");
				this.SetDirty(true);
			}
		}
	}
	public enum ActionRate{Default,FixedUpdate,Update,LateUpdate,ActionStart,ActionEnd,None};
	[AddComponentMenu("")]
	public class ActionPart : StateMonoBehaviour{
		public static Dictionary<ActionPart,bool> dirty = new Dictionary<ActionPart,bool>();
		public ActionRate rate = ActionRate.Default;
		[NonSerialized] public Action action;
		[HideInInspector] public int priority = -1;
		[HideInInspector] public bool hasReset;
		private bool requirableOverride;
		public override void Reset(){
			this.hasReset = true;
			this.Awake();
		}
		public override void Awake(){
			if(this.alias.IsEmpty()){
				this.alias = this.GetType().ToString();
			}
			if(action.IsNull()){
				this.action = this.GetComponent<Action>(true);
				if(!action.IsNull()){
					this.action.Awake();
				}
			}
			Events.Add(alias+"/End",this.End);
			Events.Add(alias+"/Start",this.Use);
			Events.Add(alias+"/End (Force)",this.ForceEnd);
			Events.Add("Action Start",this.ActionStart);
			Events.Add("Action End",this.ActionEnd);
			Events.Register("@Refresh",this.gameObject);
			Events.Register("@Update Parts",this.gameObject);
			Events.Register(this.alias+"/Disabled",this.gameObject);
			Events.Register(this.alias+"/Start",this.gameObject);
			Events.Register(this.alias+"/End",this.gameObject);
			bool controlled = this.action != null && this.action.controller != null;
			this.usable.Set(!controlled);
			this.SetDirty(true);
		}
		[ContextMenu("Toggle Column Visibility")]
		public void ToggleRequire(){
			this.requirable = !this.requirable;
			this.requirableOverride = !this.requirableOverride;
			this.gameObject.Call("@Refresh");
		}
		public virtual void Step(){
			if(!Application.isPlaying){return;}
			if(ActionPart.dirty.ContainsKey(this) && ActionPart.dirty[this]){
				this.SetDirty(false);
				this.gameObject.Call("@Update Parts");
			}
			bool actionUsable = this.action == null || this.action.usable;
			if(actionUsable && this.usable){this.Use();}
			else if(this.inUse){
				this.End();
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
		public virtual void ActionStart(){
			if(this.rate == ActionRate.ActionStart){
				this.Step();
			}
		}
		public virtual void ActionEnd(){
			if(this.rate == ActionRate.ActionEnd){
				this.Step();
			}
		}
		public void DefaultRate(string rate){
			if(this.rate == ActionRate.Default){
				if(rate == "FixedUpdate"){this.rate = ActionRate.FixedUpdate;}
				if(rate == "LateUpdate"){this.rate = ActionRate.LateUpdate;}
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
		public void SetDirty(bool state){ActionPart.dirty[this] = state;}
		public virtual void ForceEnd(){this.Toggle(false);}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "/Start" : "/End";
				this.inUse.Set(state);
				this.SetDirty(true);
				this.gameObject.Call(this.alias+active);
			}
		}
	}
}
