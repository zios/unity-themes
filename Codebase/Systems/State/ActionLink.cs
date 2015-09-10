#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
using System.Linq;
using System.Collections.Generic;
namespace Zios{
	public enum ActionOccurrence{Default,Constant,Once};
	[AddComponentMenu("")]
	public class ActionLink : StateMonoBehaviour{
		[Advanced] public ActionOccurrence occurrence = ActionOccurrence.Default;
		public bool? nextState;
		[Internal] public StateLink stateLink;
		[Internal] public ActionTable actionTable;
		public override void Awake(){
			base.Awake();
			Events.Add(this.alias+"/On End",this.End,this.gameObject);
			Events.Add(this.alias+"/On Start",this.Use,this.gameObject);
			Events.Register(this.alias+"/On Disabled",this.gameObject);
			Events.Register(this.alias+"/On Start",this.gameObject);
			Events.Register(this.alias+"/On End",this.gameObject);
			if(!Application.isPlaying){
				this.stateLink = this.GetComponent<StateLink>(true);
				this.actionTable = this.GetComponent<ActionTable>(true);
			}
			this.usable.Set(this.actionTable==null);
		}
		public override void Step(){
			if(!Application.isPlaying){return;}
			bool onlyHappenOnce = this.used && this.occurrence == ActionOccurrence.Once;
			bool stateLinkUsable = this.stateLink == null || this.stateLink.usable;
			if(!onlyHappenOnce){
				if(stateLinkUsable && this.usable){this.Use();}
				else if(this.inUse){this.End();}
			}
			else if(!this.usable){
				this.End();
			}
		}
		public override void OnDisable(){
			base.OnDisable();
			if(!this.gameObject.activeInHierarchy || !this.enabled){
				if(this.actionTable==null){this.End();}
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
				if(this.actionTable != null){
					this.nextState = state;
					return;
				}
				this.ApplyState(state);
			}
		}
		public void ApplyState(bool state){
			this.inUse.Set(state);
			this.used.Set(state);
			string active = state ? "/On Start" : "/On End";
			this.gameObject.CallEvent(this.alias+active);
			this.gameObject.CallEvent("On State Update");
		}
	}
}
