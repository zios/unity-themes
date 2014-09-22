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
		[NonSerialized] public GameObject owner;
		private string typeName;
		public virtual void Start(){
			StateController stateController = this.gameObject.GetComponentInParents<StateController>();
			if(stateController == null){
				Debug.LogError("Action ("+this.transform.name+") -- No parent StateController component found.");
				return;
			}
			this.owner = stateController.gameObject;
			string type = this.typeName = this.GetType().ToString();
			Events.AddGet("Is"+type+"Active",this.GetActive);
			Events.AddGet("Is"+type+"Usable",this.GetUsable);
			Events.Add("Disable"+type,this.OnEnableAction);
			Events.Add("Enable"+type,this.OnDisableAction);
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
			else if(!this.usable){this.End();}
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
				string active = this.inUse ? "Deactivate" : "Activate";
				this.gameObject.Call("Action"+active,this);
				this.gameObject.Call(this.typeName+active,this);
				this.inUse = state;
				if(!this.inUse){this.ready = false;}
				this.SetDirty(true);
				this.owner.Call("UpdateStates");
			}
		}
	}
	[AddComponentMenu("")][RequireComponent(typeof(Action))]
	public class ActionPart : StateMonoBehaviour{
		static public Dictionary<ActionPart,bool> dirty = new Dictionary<ActionPart,bool>();
		[NonSerialized] public Action action;
		[HideInInspector] public int priority = -1;
		[HideInInspector] public bool constant;
		[HideInInspector] public bool late;
		public override string GetInterfaceType(){return "ActionPart";}
		public virtual void OnValidate(){this.GetComponent<ActionController>().Refresh();}
		public virtual void Start(){
			this.action = this.GetComponent<Action>();
			Events.Add("ActionActivate",this.OnActionStart);
			Events.Add("ActionDeactivate",this.OnActionEnd);
			this.SetDirty(true);
		}
		public void Step(){
			if(ActionPart.dirty[this]){
				this.SetDirty(false);
				this.gameObject.Call("UpdateParts");
			}
			if(this.action.usable && this.usable){this.Use();}
			else if(this.inUse){this.End();}
		}
		public virtual void FixedUpdate(){
			if(!this.constant){
				this.Step();
			}
		}
		public virtual void Update(){
			if(this.constant && !this.late){
				this.Step();
			}
		}
		public virtual void LateUpdate(){
			if(this.constant && this.late){
				this.Step();
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
		public virtual void OnActionEnd(){
			if(this.inUse){
				this.End();
			}
		}
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