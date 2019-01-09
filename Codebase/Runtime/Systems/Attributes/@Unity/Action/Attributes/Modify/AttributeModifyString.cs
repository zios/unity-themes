using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Modify/Modify String")]
	public class AttributeModifyString : StateBehaviour{
		public AttributeString target = "";
		public AttributeString value = "";
		public override void Awake(){
			base.Awake();
			this.target.Setup("Target",this);
			this.target.info.mode = AttributeMode.Linked;
			this.value.Setup("Value",this);
		}
		public override void Use(){
			this.target.Set(this.value.Get());
			base.Use();
		}
	}
}