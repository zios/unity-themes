using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Basic")]
	public class Action : StateMonoBehaviour{
		static public Dictionary<GameObject,bool> dirty = new Dictionary<GameObject,bool>();
		public Dictionary<string,ActionPart> parts = new Dictionary<string,ActionPart>();
		public float intensity = 1.0f;
		public bool persist;
		[NonSerialized] public GameObject owner;
		public virtual void Awake(){
			StateController stateController = this.gameObject.GetComponentInParents<StateController>();
			if(stateController == null){
				Debug.LogError("Action ("+this.transform.name+") -- No parent StateController component found.");
				return;
			}
			this.owner = stateController.gameObject;
			Events.AddGet("Is"+this.alias+"Active",this.GetActive);
			Events.AddGet("Is"+this.alias+"Usable",this.GetUsable);
			Events.Add("Disable"+this.alias,this.OnEnableAction);
			Events.Add("Enable"+this.alias,this.OnDisableAction);
			Events.AddGetTarget("Is"+this.alias+"Active",this.GetActive,this.owner);
			Events.AddGetTarget("Is"+this.alias+"Usable",this.GetUsable,this.owner);
			Events.AddTarget("Enable"+this.alias,this.OnEnableAction,this.owner);
			Events.AddTarget("Disable"+this.alias,this.OnDisableAction,this.owner);
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
		public void OnEnableAction(){this.enabled = true;}
		public void OnDisableAction(){this.enabled = false;}
		public object GetActive(){return this.inUse;}
		public object GetUsable(){return this.usable;}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "Activate" : "Deactivate";
				this.gameObject.Call("Action"+active);
				//this.gameObject.Call(this.alias+active); // Call on Actions ONLY
				this.inUse = state;
				//if(!this.inUse){this.ready = false;}
				this.SetDirty(true);
				this.owner.Call("UpdateStates");
			}
		}
	}
	public enum ActionRate{FixedUpdate,Update,LateUpdate};
	[AddComponentMenu("")][RequireComponent(typeof(Action))]
	public class ActionPart : StateMonoBehaviour{
		static public Dictionary<ActionPart,bool> dirty = new Dictionary<ActionPart,bool>();
		public ActionRate rate = ActionRate.Update;
		[NonSerialized] public Action action;
		[HideInInspector] public int priority = -1;
		public override string GetInterfaceType(){return "ActionPart";}
		public virtual void OnValidate(){
			this.GetComponent<ActionController>().Refresh();
		}
		public virtual void Awake(){
			this.action = this.GetComponent<Action>();
			this.SetDirty(true);
		}
		public virtual void SetupEvents(ActionPart part){
			Events.Add("ActionActivate",part.OnActionStart);
			Events.Add("ActionDeactivate",part.OnActionEnd);
		}
		public virtual void Step(){
			if(ActionPart.dirty[this]){
				this.SetDirty(false);
				this.gameObject.Call("UpdateParts");
			}
			bool actionUsable = this.action.usable || this.action.persist && this.action.inUse;
			if(actionUsable && this.usable){this.Use();}
			else if(this.inUse){this.End();}
		}
		public virtual void FixedUpdate(){
			if(this.rate == ActionRate.FixedUpdate){
				this.Step();
			}
		}
		public virtual void Update(){
			if(this.rate == ActionRate.Update){
				this.Step();
			}
		}
		public virtual void LateUpdate(){
			if(this.rate == ActionRate.LateUpdate){
				this.Step();
			}
		}
		public void DefaultRate(string rate){
			if(this.rate == ActionRate.Update){
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
		public void SetDirty(bool state){ActionPart.dirty[this] = state;}
		public virtual void OnActionStart(){}
		public virtual void OnActionEnd(){}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				this.inUse = state;
				this.SetDirty(true);
			}
		}
	}
}