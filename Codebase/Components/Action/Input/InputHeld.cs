using System;
using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Input/Input Held")]
	public class InputHeld : StateMonoBehaviour{
		public InputRange requirement;
		public AttributeString inputName = "Button1";
		public AttributeBool heldDuringIntensity = true;
		public AttributeBool ignoreOwnership = false;
		[Advanced] public AttributeFloat manual = Mathf.Infinity;
		[Internal] public AttributeFloat intensity = 0;
		[Internal] public bool held;
		[NonSerialized] public int inputID;
		[NonSerialized] public bool lastHeld;
		[NonSerialized] public bool ownerSetup;
		public override void Awake(){
			base.Awake();
			this.inputID = this.GetInstanceID();
			this.inputName.Setup("Input Name",this);
			this.heldDuringIntensity.Setup("Held During Intensity",this);
			this.ignoreOwnership.Setup("Ignore Ownership",this);
			this.intensity.Setup("Intensity",this);
			this.manual.Setup("Manual Intensity",this);
		}
		public override void Use(){
			bool inputSuccess = this.CheckInput();
			if(inputSuccess){
				if(!this.ownerSetup){
					this.ownerSetup = true;
					if(this.ignoreOwnership || !InputState.HasOwner(this.inputName)){
						InputState.SetOwner(this.inputName,this.inputID);
					}
				}
				base.Use();
			}
			else if(this.active){
				InputState.ResetOwner(this.inputName);
				this.ownerSetup = false;
				this.lastHeld = false;
				base.End();
			}
		}
		public virtual bool CheckInput(){
			string inputName = this.inputName;
			bool isManual = this.manual.Get() != Mathf.Infinity;
			bool isOwner = this.ignoreOwnership || !InputState.HasOwner(inputName) || InputState.IsOwner(inputName,this.inputID);
			if(!isManual && !isOwner){return false;}
			float intensity = isManual ? this.manual.Get() : Input.GetAxis(inputName);
			this.held = intensity != 0;
			this.intensity.Set(intensity);
			bool released = !this.held && this.lastHeld;
			bool canEnd = (!this.heldDuringIntensity && released) || (this.heldDuringIntensity && this.intensity == 0);
			if(released && isOwner){InputState.ResetOwner(inputName);}
			if(canEnd){return false;}
			bool requirementMet = InputState.CheckRequirement(this.requirement,this.intensity);
			if(requirementMet){
				bool held = this.heldDuringIntensity ? this.intensity != 0 : this.held;
				if(!held){requirementMet = false;}
			}
			this.lastHeld = this.held;
			return requirementMet;
		}
	}
}