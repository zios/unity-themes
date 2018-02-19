using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Modify/Modify GameObject")]
	public class AttributeModifyGameObject : StateBehaviour{
		public AttributeGameObject target;
		public AttributeGameObject value;
		public override void Awake(){
			base.Awake();
			this.target.info.mode = AttributeMode.Linked;
			this.target.Setup("Target",this);
			this.value.Setup("Value",this);
		}
		public override void Use(){
			this.target.Set(this.value.Get());
			base.Use();
		}
	}
}