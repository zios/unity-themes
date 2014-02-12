using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
public enum InputType{Hold,Press,Release}
public enum InputRequirement{NotZero,LessThanZero,GreaterThanZero}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Input State")]
public class InputState : ActionPart{
	public static Dictionary<string,int> owner = new Dictionary<string,int>();
	public InputType type;
	public InputRequirement requirement;
	public string key = "*-Button1";
	public bool controlActionIntensity = false;
	public bool requireOwnership = true;
	public bool zeroOnActionEnd = true;
	[NonSerialized] public float intensity;
	[NonSerialized] public bool held;
	[NonSerialized] public bool lastHeld;
	public void Awake(){this.DefaultPriority(5);}
	public override void Start(){
		base.Start();
		this.action.AddPart(this.alias,this);
		string parentName = this.transform.parent.parent.gameObject.name.Replace("Player","P").Strip("@");
		this.key = this.key.Contains("*") ? this.key.Replace("*",parentName) : this.key;
	}
	public override void Use(){
		bool inputSuccess = this.CheckInput();
		if(inputSuccess){
			base.Use();
		}
		else{
			base.End();
		}
	}
	public override void OnActionStart(){
		if(this.requireOwnership){
			InputState.owner[this.key] = this.GetInstanceID();
		}
	}
	public override void OnActionEnd(){
		if(this.zeroOnActionEnd){
			this.intensity = 0;
		}
	}
	public virtual bool CheckInput(){
		string key = this.key;
		this.held = Input.GetAxisRaw(key) != 0;
		this.intensity = Input.GetAxis(key);
		if(this.controlActionIntensity){
			this.action.intensity = Mathf.Abs(this.intensity);
		}
		if(this.requireOwnership && InputState.owner.ContainsKey(key)){
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
		bool any = this.requirement == InputRequirement.NotZero && this.intensity != 0;
		bool less = this.requirement == InputRequirement.LessThanZero && this.intensity < 0;
		bool more = this.requirement == InputRequirement.GreaterThanZero && this.intensity > 0; 
		bool requirementMet = any || less || more;
		if(requirementMet){
			if(this.type == InputType.Hold && !this.held){requirementMet = false;}
			if(this.type == InputType.Press && this.lastHeld){requirementMet = false;}
			if(this.type == InputType.Release && !(this.lastHeld && !this.held)){requirementMet = false;}
		}
		if(!requirementMet){this.intensity = 0;}
		this.lastHeld = this.held;
		return requirementMet;
	}
}