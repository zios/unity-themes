using UnityEngine;
namespace Zios.Actions.InputComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Input/Input Pressed")]
	public class InputPressed : StateMonoBehaviour{
		public InputRange requirement;
		public AttributeString inputName = "Button1";
		[Advanced] public AttributeFloat manual = Mathf.Infinity;
		[Internal] public AttributeBool released = true;
		public override void Awake(){
			base.Awake();
			this.inputName.Setup("Input Name",this);
			this.released.Setup("Released",this);
			this.manual.Setup("Manual Intensity",this);
		}
		public override void Use(){
			bool inputSuccess = this.manual.Get() != Mathf.Infinity || this.CheckInput();
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
			if(InputState.disabled){return false;}
			float intensity = Input.GetAxis(this.inputName);
			return InputState.CheckRequirement(this.requirement,intensity);
		}
	}
}