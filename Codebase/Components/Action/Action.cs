using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using Zios;
using System;
using System.Collections.Generic;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Basic")][ExecuteInEditMode]
	public class Action : StateMonoBehaviour{
		public static float nextUpdate = 0;
		public static Dictionary<GameObject,bool> dirty = new Dictionary<GameObject,bool>();
		public bool persist;
		private bool setup;
		[NonSerialized] public StateController controller;
		[NonSerialized] public GameObject owner;
		#if UNITY_EDITOR 
		public static void EditorUpdate(){
			float time = Time.realtimeSinceStartup;
			if(Selection.activeGameObject.IsNull() || time < Action.nextUpdate){return;}
			Action.nextUpdate = time + 0.25f;
			bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
			Action current = Selection.activeGameObject.GetComponent<Action>();
			if(!current.IsNull() && !playing){
				current.Start();
			}
		}
		#endif
		public virtual void Start(){
			if(!this.setup){
				if(this.owner.IsNull()){
					this.controller = this.gameObject.GetComponentInParents<StateController>();
					this.owner = this.controller == null ? this.gameObject : this.controller.gameObject;
				}
				this.alias = this.alias.SetDefault(this.gameObject.name);
				this.inUse.Setup("Active",this,this.controller);
				this.usable.Setup("Usable",this,this.controller);
				Events.Add("Action End (Force)",this.End,this.owner);
				Events.Register("@Update Parts",this.gameObject);
				Events.Register("Action Start",this.gameObject);
				Events.Register("Action End",this.gameObject);
				if(this.owner != this.gameObject){
					Events.Register("@Update States",this.owner);
					Events.Register("@Refresh",this.owner);
					Events.Register(this.alias+"/Start",this.owner);
					Events.Register(this.alias+"/End",this.owner);
				}
				this.SetDirty(true);
				this.setup = Application.isPlaying;
			}
		}
		public virtual void Update(){
			#if UNITY_EDITOR 
			bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
			if(!playing){
				if(!EditorApplication.update.Contains((Method)Action.EditorUpdate)){
					EditorApplication.update += Action.EditorUpdate;
				}
				this.End();
			}
			#endif
		}
		public virtual void FixedUpdate(){
			if(!Application.isPlaying){return;}
			if(Action.dirty.ContainsKey(this.gameObject) && Action.dirty[this.gameObject]){
				this.SetDirty(false);
				Action.dirty[this.gameObject] = false;
				this.gameObject.Call("@Update Parts");
			}
			if(this.usable && this.ready){this.Use();}
			else if(!this.usable && !this.persist){this.End();}
		}
		public void OnDestroy(){
			if(!this.owner.IsNull()){
				this.owner.Call("@Refresh");
			}
		}
		public void SetDirty(bool state){Action.dirty[this.gameObject] = state;}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "Start" : "End";
				this.inUse = state;
				this.gameObject.Call("Action "+active);
				this.owner.Call(this.alias+" "+active);
				this.owner.Call("@Update States");
				this.SetDirty(true);
			}
		}
	}
	public enum ActionRate{Default,FixedUpdate,Update,LateUpdate,ActionStart,ActionEnd,None};
	[AddComponentMenu("")][ExecuteInEditMode]
	public class ActionPart : StateMonoBehaviour{
		public static float nextUpdate = 0;
		public static Dictionary<ActionPart,bool> dirty = new Dictionary<ActionPart,bool>();
		public ActionRate rate = ActionRate.Default;
		[NonSerialized] public Action action;
		[HideInInspector] public int priority = -1;
		private bool requirableOverride;
		public override string GetInterfaceType(){return "ActionPart";}
		#if UNITY_EDITOR 
		public static void EditorUpdate(){
			float time = Time.realtimeSinceStartup;
			if(Selection.activeGameObject.IsNull() || time < ActionPart.nextUpdate){return;}
			ActionPart.nextUpdate = time + 0.25f;
			bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
			ActionPart current = Selection.activeGameObject.GetComponent<ActionPart>();
			if(!current.IsNull() && !playing){
				var instances = (ActionPart[])current.gameObject.GetComponents<ActionPart>();
				foreach(ActionPart part in instances){
					part.Start();
				}
			}
		}
		#endif
		public virtual void Start(){
			if(this.alias.IsEmpty()){
				this.alias = this.GetType().ToString();
			}
			if(action.IsNull()){
				this.action = this.GetComponent<Action>();
				if(!action.IsNull()){
					this.action.Start();
				}
			}
			Events.Add(alias+"/End",this.End);
			Events.Add(alias+"/Start",this.Use);
			Events.Add(alias+"/End (Force)",this.ForceEnd);
			Events.Add("Action Start",this.ActionStart);
			Events.Add("Action End",this.ActionEnd);
			Events.Register("@Refresh",this.gameObject);
			Events.Register("@Update Parts",this.gameObject);
			Events.Register(this.alias+"/Start",this.gameObject);
			Events.Register(this.alias+"/End",this.gameObject);
			this.usable = true;
			this.SetDirty(true);
		}
		[ContextMenu("Toggle Column Visibility")]
		public void ToggleRequire(){
			this.requirable = !this.requirable;
			this.requirableOverride = !this.requirableOverride;
			this.gameObject.Call("@Refresh");
		}
		public virtual void Step(){
			if(!Application.isPlaying){return;}
			if(ActionPart.dirty.ContainsKey(this) && ActionPart.dirty[this]){
				this.SetDirty(false);
				this.gameObject.Call("@Update Parts");
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
			#if UNITY_EDITOR 
			bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
			if(!playing){
				if(!EditorApplication.update.Contains((Method)ActionPart.EditorUpdate)){
					EditorApplication.update += ActionPart.EditorUpdate;
				}
				this.inUse = false;
			}
			#endif
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
		public void OnDestroy(){
			if(!this.gameObject.IsNull()){
				this.gameObject.Call("@Refresh");
			}
		}
		public void SetDirty(bool state){ActionPart.dirty[this] = state;}
		public virtual void ForceEnd(){this.Toggle(false);}
		public override void Use(){this.Toggle(true);}
		public override void End(){this.Toggle(false);}
		public override void Toggle(bool state){
			if(state != this.inUse){
				string active = state ? "/Start" : "/End";
				this.inUse = state;
				this.gameObject.Call(this.alias+active);
				this.SetDirty(true);
			}
		}
	}
}
