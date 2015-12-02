using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Attribute/Modify/Modify Bool")]
	public class AttributeModifyBool : StateMonoBehaviour{
		public AttributeBool target = false;
		public AttributeBool value = false;
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