#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/*/State Link")]
	public class StateLink : StateMonoBehaviour{
		[NonSerialized] public StateTable stateTable;
		[NonSerialized] public GameObject owner;
		public override void Awake(){
			string name = this.transform.parent != null ? this.transform.parent.name : this.transform.name;
			this.alias = this.alias.SetDefault(name);
			base.Awake();
			this.AddDependent<ActionReady>();
			GameObject parent = this.gameObject.GetParent();
			this.stateTable = parent.IsNull() ? null : parent.GetComponentInParent<StateTable>(true);
			this.owner = this.stateTable == null ? this.gameObject : this.stateTable.gameObject;
			if(!this.stateTable.IsNull()){
				this.inUse.AddScope(this.stateTable);
				this.usable.AddScope(this.stateTable);
			}
			Events.Register("@Update States",this.gameObject);
			Events.Register("Action Start",this.gameObject);
			Events.Register("Action End",this.gameObject);
			Events.Register("Action Disabled",this.gameObject);
			if(this.owner != this.gameObject){
				Events.Register("@Update States",this.owner);
				Events.Register("@Refresh",this.owner);
				Events.Register(this.alias+"/Start",this.owner);
				Events.Register(this.alias+"/End",this.owner);
			}
			this.usable.Set(this.stateTable==null);
			this.ready.Set(false);
		}
		public override void Step(){
			if(!Application.isPlaying){return;}
			if(this.usable && this.ready){this.Use();}
			else if(!this.usable){this.End();}
		}
		public void OnDisable(){
			this.gameObject.CallEvent("Action Disabled");
			this.gameObject.CallEvent("@Update States");
		}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "Start" : "End";
				this.inUse.Set(state);
				this.gameObject.CallEvent("Action "+active);
				this.owner.CallEvent(this.alias+" "+active);
				this.owner.CallEvent("@Update States");
				this.gameObject.CallEvent("@Update States");
			}
		}
	}
}
