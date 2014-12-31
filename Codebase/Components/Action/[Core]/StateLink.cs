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
			base.Awake();
			GameObject parent = this.gameObject.GetParent();
			this.alias = this.gameObject.name;
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
		public void OnDestroy(){
			if(!this.owner.IsNull()){
				this.owner.Call("@Refresh");
			}
		}
		public void OnDisable(){
			this.gameObject.Call("Action Disabled");
			this.gameObject.Call("@Update States");
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
				this.gameObject.Call("@Update States");
			}
		}
	}
}
