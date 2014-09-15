using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Input Held")]
public class InputHeld : ActionPart{
	public InputRange requirement;
	public string key = "Button1";
	public bool controlActionIntensity = true;
	public bool heldDuringIntensity = true;
	public bool shared = false;
	[NonSerialized] public bool held;
	[NonSerialized] public bool lastHeld;
	public void OnValidate(){this.DefaultPriority(5);}
	public override void Start(){
		base.Start();
		this.action.AddPart(this.alias,this);
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
			InputState.owner[this.key] = this.GetInstanceID();
		}
	}
	public override void OnActionEnd(){}
	public virtual bool CheckInput(){
		string key = this.key;
		int id = this.GetInstanceID();
		this.held = Input.GetAxisRaw(key) != 0;
		float intensity = Input.GetAxis(key);
		bool released = this.held != this.lastHeld;
		bool canEnd = !this.heldDuringIntensity || (this.heldDuringIntensity && intensity == 0);
		if(canEnd && !this.shared && InputState.CheckOwner(key,id,released)){
			return false;
		}
		bool requirementMet = InputState.CheckRequirement(this.requirement,intensity);
		if(requirementMet){
			bool held = this.heldDuringIntensity ? intensity != 0 : this.held;
			if(!held){requirementMet = false;}
		}
		if(requirementMet && this.controlActionIntensity){this.action.intensity = Mathf.Abs(intensity);}
		this.lastHeld = this.held;
		return requirementMet;
	}
}