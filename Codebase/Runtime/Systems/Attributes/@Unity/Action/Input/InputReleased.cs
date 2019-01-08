using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Input/Input Released")]
	public class InputReleased : StateBehaviour{
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