using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Input Pressed")]
    public class InputPressed : ActionLink{
	    public InputRange requirement;
	    public AttributeString inputName = "Button1";
	    [Internal] public AttributeBool released = true;
	    [NonSerialized] public bool setup;
	    public override void Awake(){
		    base.Awake();
		    this.inputName.Setup("Input Name",this);
		    this.released.Setup("Released",this);
	    }
	    public override void Use(){
		    bool inputSuccess = this.CheckInput();
			bool released = this.released.Get();
		    if(inputSuccess && released){
				this.released.Set(false);
				base.Use();
		    }
		    if(!inputSuccess && !released){
				this.released.Set(true);
		    }
			base.End();
	    }
	    public virtual bool CheckInput(){
		    float intensity = Input.GetAxis(this.inputName);
		    return InputState.CheckRequirement(this.requirement,intensity);
	    }
    }
}
