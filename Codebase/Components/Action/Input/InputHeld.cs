using System;
using UnityEngine;
namespace Zios.Actions.InputComponents{
	using Attributes;
	using Inputs;
	using Events;
	[AddComponentMenu("Zios/Component/Action/Input/Input Held")]
	public class InputHeld : StateMonoBehaviour{
		[Advanced] public InputRange requirement;
		public AttributeGameObject target;
		[Advanced] public AttributeBool heldDuringIntensity = true;
		[Internal] public bool held;
		[Internal] public AttributeString inputName = "Button1";
		[Internal] public AttributeFloat intensity = 0;
		[NonSerialized] public InputInstance instance;
		private bool lastHeld;
		public override void Awake(){
			base.Awake();
			this.inputName.Setup("Input Name",this);
			this.target.Setup("Input Target",this);
			this.intensity.Setup("Input Intensity",this);
			this.heldDuringIntensity.Setup("Input Held During Intensity",this);
			this.AddDependent<InputInstance>(target);
			this.SetInstance();
			Event.Add("On Validate",this.SetInstance,this);
		}
		public void SetInstance(){
			this.instance = this.target.Get() ? this.target.Get().GetComponent<InputInstance>() : null;
		}
		public override void Use(){
			bool inputSuccess = this.CheckInput();
			if(inputSuccess){
				base.Use();
			}
			else if(this.active){
				this.lastHeld = false;
				base.End();
			}
		}
		public virtual bool CheckInput(){
			if(InputState.disabled || this.instance.IsNull()){return false;}
			float intensity = this.instance.GetIntensity(this.inputName);
			this.held = this.instance.GetHeld(this.inputName);
			this.intensity.Set(intensity);
			bool released = !this.held && this.lastHeld;
			bool canEnd = (!this.heldDuringIntensity && released) || (this.heldDuringIntensity && this.intensity == 0);
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