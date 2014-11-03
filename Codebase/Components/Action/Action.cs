using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Basic")]
	public class Action : StateMonoBehaviour{
		static public Dictionary<GameObject,bool> dirty = new Dictionary<GameObject,bool>();
		public Dictionary<string,ActionPart> parts = new Dictionary<string,ActionPart>();
		public bool persist;
		[NonSerialized] public GameObject owner;
		public void OnValidate(){
			StateController stateController = this.gameObject.GetComponentInParents<StateController>();
			this.inUse.Setup(alias+" Active",this,stateController);
			this.usable.Setup(alias+" Usable",this,stateController);
			this.owner = stateController == null ? this.gameObject : stateController.gameObject;
			this.gameObject.Call("Refresh");
		}
		public virtual void Awake(){
			this.OnValidate();
			string alias = this.alias.Strip(" ");
			Events.Add("ForceEndAction",this.End);
			Events.Add("ForceEnd"+alias,this.End);
			Events.AddScope("ForceEnd"+alias,this.End,this.owner);
			this.owner.Call("UpdateStates");
			this.owner.CallChildren("UpdateParts");
			this.SetDirty(true);
		}
		public virtual void FixedUpdate(){
			if(Action.dirty.ContainsKey(this.gameObject) && Action.dirty[this.gameObject]){
				this.SetDirty(false);
				Action.dirty[this.gameObject] = false;
				this.gameObject.Call("UpdateParts");
			}
			if(this.usable && this.ready){this.Use();}
			else if(!this.usable && !this.persist){this.End();}
		}
		public void SetDirty(bool state){Action.dirty[this.gameObject] = state;}
		public void AddPart(string name,ActionPart part){this.parts[name] = part;}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "Start" : "End";
				this.inUse = state;
				this.gameObject.Call("Action"+active);
				this.gameObject.Call(this.alias.Strip(" ")+active);
				this.owner.Call(this.alias.Strip(" ")+active);
				this.SetDirty(true);
				this.owner.Call("UpdateStates");
			}
		}
	}
	public enum ActionRate{Default,FixedUpdate,Update,LateUpdate,ActionStart,ActionEnd,None};
	[AddComponentMenu("")]
	public class ActionPart : StateMonoBehaviour{
		static public Dictionary<ActionPart,bool> dirty = new Dictionary<ActionPart,bool>();
		public ActionRate rate = ActionRate.Default;
		[NonSerialized] public Action action;
		[HideInInspector] public int priority = -1;
		private bool requirableOverride;
		public override string GetInterfaceType(){return "ActionPart";}
		public void Reset(){this.OnValidate();}
		public virtual void OnValidate(){
			if(this.alias.IsEmpty()){
				this.alias = this.GetType().ToString();
			}
			this.action = this.GetComponent<Action>();
			if(this.action != null){
				this.action.OnValidate();
			}
			this.usable = true;
		}
		public virtual void Awake(){
			this.OnValidate();
			string alias = this.alias.Strip(" ");
			Events.Add("End"+alias,this.End);
			Events.Add("Use"+alias,this.Use);
			Events.Add("ForceEnd"+alias,this.ForceEnd);
			Events.Add("ActionStart",this.ActionStart);
			Events.Add("ActionEnd",this.ActionEnd);
			this.SetDirty(true);
		}
		[ContextMenu("Toggle Column Visibility")]
		public void ToggleRequire(){
			this.requirable = !this.requirable;
			this.requirableOverride = !this.requirableOverride;
			this.gameObject.Call("Refresh");
		}
		public virtual void Step(){
			if(ActionPart.dirty[this]){
				this.SetDirty(false);
				this.gameObject.Call("UpdateParts");
			}
			bool actionUsable = this.action == null || (this.action.usable || (this.action.persist && this.action.inUse));
			if(actionUsable && this.usable){this.Use();}
			else if(this.inUse){this.End();}
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
		public virtual void ActionStart(){
			if(this.rate == ActionRate.ActionStart){
				this.Step();
			}
		}
		public virtual void ActionEnd(){
			if(this.rate == ActionRate.ActionEnd){
				this.Step();
			}
			this.End();
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
				string active = state ? "Start" : "End";
				this.inUse = state;
				this.gameObject.Call(this.alias.Strip(" ")+active);
				this.SetDirty(true);
			}
		}
	}
}
