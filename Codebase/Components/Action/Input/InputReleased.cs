using UnityEngine;
namespace Zios.Actions.InputComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Input/Input Released")]
	public class InputReleased : StateMonoBehaviour{
		public AttributeString inputName = "Button1";
		public override void Awake(){
			base.Awake();
			this.inputName.Setup("Input Name",this);
		}
		public override void Use(){
			bool inputHeld = Input.GetAxisRaw(this.inputName) != 0;
			if(!inputHeld){base.Use();}
			else{base.End();}
		}
	}
}