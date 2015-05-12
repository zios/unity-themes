#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/*/State Link")]
	public class StateLink : StateMonoBehaviour{
		[Internal] public StateTable stateTable;
		[Internal] public GameObject owner;
		public override void Awake(){
			string name = this.transform.parent != null ? this.transform.parent.name : this.transform.name;
			this.alias = this.alias.SetDefault(name);
			base.Awake();
			if(!Application.isPlaying){
				this.AddDependent<ActionReady>();
				GameObject parent = this.gameObject.GetParent();
				this.stateTable = parent.IsNull() ? null : parent.GetComponentInParent<StateTable>(true);
				this.owner = this.gameObject.GetPrefabRoot();
				if(!this.stateTable.IsNull()){
					this.inUse.AddScope(this.stateTable);
					this.usable.AddScope(this.stateTable);
				}
				Events.Register("On State Update",this.gameObject);
				Events.Register("On Action Start",this.gameObject);
				Events.Register("On Action End",this.gameObject);
				Events.Register("On Action Disabled",this.gameObject);
				if(this.owner != this.gameObject){
					Events.Register("On State Update",this.owner);
					Events.Register("On State Refresh",this.owner);
					Events.Register(this.alias+"/On Start",this.owner);
					Events.Register(this.alias+"/On End",this.owner);
				}
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
			this.gameObject.CallEvent("On Action Disabled");
			this.gameObject.CallEvent("On State Update");
		}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "Start" : "End";
				this.inUse.Set(state);
				this.gameObject.CallEvent("On Action "+active);
				this.owner.CallEvent(this.alias + "/On " + active);
				this.owner.CallEvent("On State Update");
				this.gameObject.CallEvent("On State Update");
			}
		}
	}
}
