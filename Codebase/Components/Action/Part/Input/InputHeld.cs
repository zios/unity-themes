using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
[AddComponentMenu("Zios/Component/Action/Part/Input Held")]
public class InputHeld : ActionPart{
	public AttributeString inputName = "Button1";
	public InputRange requirement;
	public AttributeBool forcePositiveIntensity = true;
	public AttributeBool heldDuringIntensity = true;
	public AttributeBool exclusive = false;
	[HideInInspector] public AttributeFloat intensity = 0;
	[HideInInspector] public bool held;
	[NonSerialized] public bool lastHeld;
	public override void Start(){
		base.Start();
		this.DefaultPriority(5);
		this.inputName.Setup("InputName",this);
		this.forcePositiveIntensity.Setup("Force Positive Intensity",this);
		this.heldDuringIntensity.Setup("Held During Intensity",this);
		this.exclusive.Setup("Exclusive",this);
		this.intensity.Setup("Intensity",this);
		Events.Add("ActionStart",this.OnActionStart);
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
	public void OnActionStart(){
		if(this.exclusive){
			InputState.owner[this.inputName] = this.GetInstanceID();
		}
	}
	public virtual bool CheckInput(){
		string inputName = this.inputName;
		int id = this.GetInstanceID();
		this.held = Input.GetAxisRaw(inputName) != 0;
		float intensity = this.forcePositiveIntensity ? Mathf.Abs(Input.GetAxis(inputName)) : Input.GetAxis(inputName);
		this.intensity.Set(intensity);
		bool released = this.held != this.lastHeld;
		bool canEnd = !this.heldDuringIntensity || (this.heldDuringIntensity && intensity == 0);
		if(canEnd && this.exclusive && InputState.CheckOwner(inputName,id,released)){
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
