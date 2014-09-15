using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
public enum InputType{Hold,Press,Release}
public enum RequireRange{NotZero,Zero,LessThanZero,GreaterThanZero}
public enum InputRange{None,LessThanEqualZero,GreaterThanEqualZero}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Input State")]
public class InputState : ActionPart{
	public static Dictionary<string,int> owner = new Dictionary<string,int>();
	public InputType type;
	public RequireRange requirement;
	public InputRange clampRange;
	public string key = "*-Button1";
	public bool controlActionIntensity = false;
	public bool heldDuringIntensity = false;
	public bool requireOwnership = true;
	[NonSerialized] public bool held;
	[NonSerialized] public bool lastHeld;
	[NonSerialized] public float intensity;
	public void OnValidate(){this.DefaultPriority(5);}
	public override void Start(){
		base.Start();
		this.action.AddPart(this.alias,this);
		if(this.transform.parent != null && this.transform.parent.parent != null){
			string parentName = this.transform.parent.parent.gameObject.name.Replace("Player","P").Strip("@");
			this.key = this.key.Contains("*") ? this.key.Replace("*",parentName) : this.key;
		}
	}
	public override void Use(){
		bool inputSuccess = this.CheckInput();
		if(inputSuccess){
			base.Use();
		}
		else if(this.inUse){
			base.End();
		}
	}
	public override void OnActionStart(){
		if(this.requireOwnership){
			InputState.owner[this.key] = this.GetInstanceID();
		}
	}
	public override void OnActionEnd(){}
	public virtual bool CheckInput(){
		string key = this.key;
		this.held = Input.GetAxisRaw(key) != 0;
		this.intensity = Input.GetAxis(key);
		bool canEnd = !this.heldDuringIntensity || (this.heldDuringIntensity && this.intensity == 0);
		if(this.clampRange == InputRange.LessThanEqualZero){this.intensity = Mathf.Clamp(this.intensity,-1,0);}
		if(this.clampRange == InputRange.GreaterThanEqualZero){this.intensity = Mathf.Clamp(this.intensity,0,1);}
		if(canEnd && this.requireOwnership && InputState.owner.ContainsKey(key)){
			int owner = InputState.owner[key];
			bool isOwner = owner == this.GetInstanceID();
			if(isOwner && this.held != this.lastHeld){
				InputState.owner[key] = -1;
				return false;
			}
			if(!isOwner && owner != -1){
				return false;
			}
		}
		bool none = this.requirement == RequireRange.Zero && this.intensity == 0; 
		bool any = this.requirement == RequireRange.NotZero && this.intensity != 0;
		bool less = this.requirement == RequireRange.LessThanZero && this.intensity < 0;
		bool more = this.requirement == RequireRange.GreaterThanZero && this.intensity > 0; 
		bool requirementMet = any || less || more || none;
		if(requirementMet){
			bool held = this.heldDuringIntensity ? this.intensity != 0 : this.held;
			if(this.type == InputType.Hold && !held){requirementMet = false;}
			if(this.type == InputType.Press && lastHeld){requirementMet = false;}
			if(this.type == InputType.Release && !(lastHeld && !held)){requirementMet = false;}
		}
		if(requirementMet && this.controlActionIntensity){this.action.intensity = Mathf.Abs(this.intensity);}
		if(!requirementMet){this.intensity = 0;}
		this.lastHeld = this.held;
		return requirementMet;
	}
}