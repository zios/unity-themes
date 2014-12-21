#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
using System.Linq;
using System.Collections.Generic;
namespace Zios{
	public enum ActionOccurrence{Default,Constant,Once};
	[AddComponentMenu("")]
	public class ActionPart : StateMonoBehaviour{
		public ActionOccurrence occurrence = ActionOccurrence.Default;
		public bool? nextState;
		[NonSerialized] public Action action;
		[NonSerialized] public ActionController controller;
		public override void Awake(){
			base.Awake();
			Events.Add(this.alias+"/End",this.End);
			Events.Add(this.alias+"/Start",this.Use);
			Events.Register("@Refresh",this.gameObject);
			Events.Register(this.alias+"/Disabled",this.gameObject);
			Events.Register(this.alias+"/Started",this.gameObject);
			Events.Register(this.alias+"/Ended",this.gameObject);
			if(Application.isPlaying){
				if(this.action.IsNull()){
					this.action = this.GetComponent<Action>(true);
					if(!this.action.IsNull()){
						this.action.Awake();
					}
				}
				this.controller = this.GetComponent<ActionController>(true);
				this.usable.Set(this.controller==null);
			}
		}
		public override void Step(){
			if(!Application.isPlaying){return;}
			bool happenedOnce = this.used && this.occurrence == ActionOccurrence.Once;
			bool actionUsable = this.action == null || this.action.usable;
			if(!happenedOnce){
				if(actionUsable && this.usable){this.Use();}
				else if(this.inUse || this.endWhileUnusable){this.End();}
			}
			else if(!this.usable){
				this.End();
			}
		}
		public void OnDestroy(){
			if(!this.gameObject.IsNull()){
				this.gameObject.Call("@Refresh");
			}
		}
		public void OnDisable(){
			if(!this.gameObject.activeInHierarchy || !this.enabled){
				this.gameObject.Call(this.alias+"/Disabled");
				if(this.controller==null){this.End();}
			}
		}
		public void DefaultOccurrence(string occurrence){
			if(this.occurrence == ActionOccurrence.Default){
				if(occurrence == "Constant"){this.occurrence = ActionOccurrence.Constant;}
				if(occurrence == "Once"){this.occurrence = ActionOccurrence.Once;}
				//if(occurrence == "Never"){this.occurrence = ActionOccurrence.Never;}
			}
		}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			bool onceReset = this.used && this.occurrence == ActionOccurrence.Once && !state;
			if(onceReset || (state != this.inUse)){
				if(this.controller != null){
					this.nextState = state;
					return;
				}
				this.ApplyState(state);
			}
		}
		public void ApplyState(bool state){
			this.inUse.Set(state);
			this.used.Set(state);
			string active = state ? "/Started" : "/Ended";
			this.gameObject.Call(this.alias+active);
		}
	}
}