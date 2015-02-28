using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Input Held")]
    public class InputHeld : ActionLink{
	    public InputRange requirement;
	    public AttributeString inputName = "Button1";
	    public AttributeFloat intensity = 0;
	    public AttributeBool heldDuringIntensity = true;
	    public AttributeBool ignoreOwnership = false;
	    [HideInInspector] public bool held;
	    [NonSerialized] public int inputID;
	    [NonSerialized] public bool lastHeld;
	    [NonSerialized] public bool setup;
	    public override void Awake(){
		    base.Awake();
		    this.inputID = this.GetInstanceID();
		    this.inputName.Setup("Input Name",this);
		    this.heldDuringIntensity.Setup("Held During Intensity",this);
		    this.ignoreOwnership.Setup("Ignore Ownership",this);
		    this.intensity.Setup("Intensity",this);
	    }
	    public override void Use(){
		    bool inputSuccess = this.CheckInput();
		    if(inputSuccess){
			    if(!this.setup){
				    this.setup = true;
				    if(this.ignoreOwnership || !InputState.HasOwner(this.inputName)){
					    InputState.SetOwner(this.inputName,this.inputID);
				    }
			    }
			    base.Use();
		    }
		    else if(this.inUse){
			    InputState.ResetOwner(this.inputName);
			    this.setup = false;
			    this.lastHeld = false;
			    base.End();
		    }
	    }
	    public virtual bool CheckInput(){
		    string inputName = this.inputName;
		    bool isOwner = this.ignoreOwnership || !InputState.HasOwner(inputName) || InputState.IsOwner(inputName,this.inputID);
		    if(!isOwner){return false;}
		    this.held = Input.GetAxisRaw(inputName) != 0;
		    this.intensity.Set(Input.GetAxis(inputName));
		    bool released = !this.held && this.lastHeld;
		    bool canEnd = (!this.heldDuringIntensity && released) || (this.heldDuringIntensity && intensity == 0);
		    if(released && isOwner){InputState.ResetOwner(inputName);}
		    if(canEnd){return false;}
		    bool requirementMet = InputState.CheckRequirement(this.requirement,intensity);
		    if(requirementMet){
			    bool held = this.heldDuringIntensity ? intensity != 0 : this.held;
			    if(!held){requirementMet = false;}
		    }
		    this.lastHeld = this.held;
		    return requirementMet;
	    }
    }
}