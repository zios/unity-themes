using System;
using UnityEngine;
namespace Zios.Actions.InputComponents{
	using Attributes;
	using Inputs;
	using Event;
	[AddComponentMenu("Zios/Component/Action/Input/Input Pressed")]
	public class InputPressed : StateMonoBehaviour{
		[Advanced] public InputRange requirement;
		public AttributeGameObject target;
		[InputName] public AttributeString inputName = "";
		[Internal] public AttributeBool released = true;
		[NonSerialized] public InputInstance instance;
		public override void Awake(){
			base.Awake();
			this.target.Setup("Input Target",this);
			this.inputName.Setup("Input Name",this);
			this.released.Setup("Released",this);
			this.AddDependent<InputInstance>(target);
			this.SetInstance();
			Events.Add("On Validate",this.SetInstance,this);
		}
		public void SetInstance(){
			this.instance = this.target.Get() ? this.target.Get().GetComponent<InputInstance>() : null;
		}
		public override void Use(){
			bool inputSuccess = this.CheckInput();
			bool released = this.released;
			if(inputSuccess && released){
				this.released.Set(false);
				base.Use();
				return;
			}
			if(!inputSuccess && !released){
				this.released.Set(true);
			}
			base.End();
		}
		public virtual bool CheckInput(){
			if(InputState.disabled || this.instance.IsNull()){return false;}
			float intensity = this.instance.GetIntensity(this.inputName);
			return InputState.CheckRequirement(this.requirement,intensity);
		}
	}
}