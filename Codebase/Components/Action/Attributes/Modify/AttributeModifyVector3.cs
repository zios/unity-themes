using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Attribute/Modify/Modify Vector3")]
	public class AttributeModifyVector3 : StateMonoBehaviour{
		public AttributeVector3 target = Vector3.zero;
		public AttributeVector3 value = Vector3.zero;
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