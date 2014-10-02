using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Input Held")]
public class InputHeld : ActionPart{
	public string inputName = "Button1";
	public InputRange requirement;
	public bool controlActionIntensity = true;
	public bool forcePositiveIntensity = true;
	public bool heldDuringIntensity = true;
	public bool shared = false;
	[NonSerialized] public bool held;
	[NonSerialized] public bool lastHeld;
	public override void OnValidate(){
		this.DefaultPriority(5);
		base.OnValidate();
	}
	public void Start(){
		this.SetupEvents(this);
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
		if(!this.shared){
			InputState.owner[this.inputName] = this.GetInstanceID();
		}
	}
	public virtual bool CheckInput(){
		string inputName = this.inputName;
		int id = this.GetInstanceID();
		this.held = Input.GetAxisRaw(inputName) != 0;
		float intensity = Input.GetAxis(inputName);
		bool released = this.held != this.lastHeld;
		bool canEnd = !this.heldDuringIntensity || (this.heldDuringIntensity && intensity == 0);
		if(this.controlActionIntensity){
			this.action.intensity = this.forcePositiveIntensity ? Mathf.Abs(intensity) : intensity;
		}
		if(canEnd && !this.shared && InputState.CheckOwner(inputName,id,released)){
			return false;
		}
		bool requirementMet = InputState.CheckRequirement(this.requirement,intensity);
		if(requirementMet){
			bool held = this.heldDuringIntensity ? intensity != 0 : this.held;
			if(!held){requirementMet = false;}
		}
		this.lastHeld = this.held;
		return requirementMet;
	}
}