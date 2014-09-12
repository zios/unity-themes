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
			this.owner = this.gameObject.GetComponentInParents<StateController>().gameObject;
			string type = this.typeName = this.GetType().ToString();
			Events.AddGet("Is"+type+"Active",this.GetActive);
			Events.AddGet("Is"+type+"Usable",this.GetUsable);
			Events.Add("Disable"+type,this.OnEnableAction);
			Events.Add("Enable"+type,this.OnDisableAction);
			this.SetDirty(true);
		}
		public virtual void FixedUpdate(){
			if(Action.dirty[this.gameObject]){
				this.SetDirty(false);
				ActionPart.dirty[this] = false;
				this.owner.Call("UpdateStates");
				this.owner.CallChildren("UpdateParts");
			}
			if(this.usable && this.ready){this.Use();}
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
				this.gameObject.CallChildren("Action"+active,this);
				this.gameObject.CallChildren(this.typeName+active,this);
				this.inUse = state;
				this.SetDirty(true);
			}
		}
	}
	[AddComponentMenu("")][RequireComponent(typeof(Action))]
	public class ActionPart : StateMonoBehaviour{
		static public Dictionary<Action,bool> dirty = new Dictionary<Action,bool>();
		[NonSerialized] public Action action;
		[HideInInspector] public int priority = -1;
		public override string GetInterfaceType(){return "ActionPart";}
		public virtual void Start(){
			this.action = this.GetComponent<Action>();
			Events.Add("ActionActivate",this.OnActionStart);
			Events.Add("ActionDeactivate",this.OnActionEnd);
			this.SetDirty(true);
		}
		public virtual void FixedUpdate(){
			if(ActionPart.dirty[this.action]){
				this.SetDirty(false);
				this.gameObject.Call("UpdateParts");
			}
			if(this.usable){this.Use();}
			else if(this.inUse){this.End();}
		}
		public void DefaultPriority(int priority){
			if(this.priority == -1){
				this.priority = priority;
			}
		}
		public void SetDirty(bool state){ActionPart.dirty[this.action] = state;}
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