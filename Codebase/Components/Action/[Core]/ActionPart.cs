#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
namespace Zios{
	public enum ActionOccurrence{Default,Constant,Once};
	[AddComponentMenu("")]
	public class ActionPart : StateMonoBehaviour{
		public static Dictionary<GameObject,bool> dirty = new Dictionary<GameObject,bool>();
		public static Dictionary<ActionPart,List<ActionPart>> once = new Dictionary<ActionPart,List<ActionPart>>();
		public static ActionPart current;
		public ActionOccurrence occurrence = ActionOccurrence.Default;
		[NonSerialized] public Action action;
		[HideInInspector] public int priority = -1;
		[HideInInspector] public bool hasReset;
		private bool requirableOverride;
		public override void Reset(){
			this.hasReset = true;
			this.Awake();
		}
		public override void Awake(){
			base.Awake();
			if(action.IsNull()){
				this.action = this.GetComponent<Action>(true);
				if(!action.IsNull()){
					this.action.Awake();
				}
			}
			Events.Add(this.alias+"/End",this.End);
			Events.Add(this.alias+"/Start",this.Use);
			Events.Add(this.alias+"/End (Force)",this.ForceEnd);
			Events.Register("@Refresh",this.gameObject);
			Events.Register("@Update Parts",this.gameObject);
			Events.Register(this.alias+"/Disabled",this.gameObject);
			Events.Register(this.alias+"/Started",this.gameObject);
			Events.Register(this.alias+"/Ended",this.gameObject);
			bool controlled = this.GetComponent<ActionController>(true) != null;
			this.usable.Set(!controlled);
			this.SetDirty(true);
			if(Application.isPlaying){
				ActionPart.once[this] = new List<ActionPart>();
			}
		}
		[ContextMenu("Toggle Column Visibility")]
		public void ToggleRequire(){
			this.requirable = !this.requirable;
			this.requirableOverride = !this.requirableOverride;
			this.gameObject.Call("@Refresh");
		}
		public override void Step(){
			if(!Application.isPlaying){return;}
			ActionPart.current = this;
			if(ActionPart.once[this].Count > 0){
				foreach(ActionPart delay in ActionPart.once[this]){
					delay.inUse.Set(false);
					delay.SetDirty(true);
				}
				ActionPart.once[this].Clear();
			}
			if(ActionPart.dirty[this.gameObject]){
				this.gameObject.Call("@Update Parts");
				this.SetDirty(false);
			}
			bool happenedOnce = this.used && this.occurrence == ActionOccurrence.Once;
			bool actionUsable = this.action == null || this.action.usable;
			if(!happenedOnce){
				if(actionUsable && this.usable){this.Use();}
				else if(this.inUse){
					this.End();
				}
			}
			if(this.used && !this.usable){
				this.End();
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

		public void DefaultOccurrence(string occurrence){
			if(this.occurrence == ActionOccurrence.Default){
				if(occurrence == "Constant"){this.occurrence = ActionOccurrence.Constant;}
				if(occurrence == "Once"){this.occurrence = ActionOccurrence.Once;}
				//if(occurrence == "Never"){this.occurrence = ActionOccurrence.Never;}
			}
		}
		public void DefaultPriority(int priority){
			if(this.priority == -1){
				this.priority = priority;
			}
		}
		public void DefaultAlias(string name){
			if(string.IsNullOrEmpty(this.alias)){
				this.alias = name;
				this.alias = name;
			}
		}
		public void DefaultRequirable(bool state){
			if(!this.requirableOverride){
				this.requirable = state;
			}
		}
		public virtual void ForceEnd(){this.Toggle(false,true);}
		public virtual void SetDirty(bool state){ActionPart.dirty[this.gameObject] = state;}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public void Toggle(bool state,bool force=false){
			bool onceReset = this.used && this.occurrence == ActionOccurrence.Once;
			if(onceReset || state != this.inUse || force){
				string active = state ? "/Started" : "/Ended";
				this.used = state;
				this.inUse.Set(state);
				this.gameObject.Call(this.alias+active);
				if(state && this.occurrence == ActionOccurrence.Once){
					if(!ActionPart.once[ActionPart.current].Contains(this)){
						ActionPart.once[ActionPart.current].Add(this);
					}
				}
				this.SetDirty(true);
			}
		}
	}
}