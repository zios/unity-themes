using System;
using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Inputs;
	using Zios.State;
	using Zios.SystemAttributes;
	using Zios.Unity.SystemAttributes;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	//asm Zios.Unity.Shortcuts;
	[AddComponentMenu("Zios/Component/Action/Input/Input Held")]
	public class InputHeld : StateBehaviour{
		[Advanced] public InputRange requirement;
		public AttributeGameObject target;
		[InputName] public AttributeString inputName = "";
		[Advanced] public AttributeBool heldDuringIntensity = true;
		[Internal] public AttributeFloat intensity = 0;
		[NonSerialized] public InputInstance instance;
		public override void Awake(){
			base.Awake();
			this.inputName.Setup("Input Name",this);
			this.target.Setup("Input Target",this);
			this.intensity.Setup("Input Intensity",this);
			this.heldDuringIntensity.Setup("Input Held During Intensity",this);
			this.AddDependent<InputInstance>(target);
			this.SetInstance();
			Events.Add("On Validate",this.SetInstance,this);
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
				base.End();
			}
		}
		public virtual bool CheckInput(){
			if(InputState.disabled || this.instance.IsNull()){
				this.intensity.Set(0);
				return false;
			}
			float intensity = this.instance.GetIntensity(this.inputName);
			bool valid = (this.heldDuringIntensity && intensity != 0) || (!this.heldDuringIntensity && this.instance.GetHeld(this.inputName));
			this.intensity.Set(intensity);
			return valid && InputState.CheckRequirement(this.requirement,this.intensity);
		}
	}
}