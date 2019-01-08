using UnityEngine;
namespace Zios.Attributes.Deprecated.Rotate{
	using Zios.Attributes.Supports;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Rotate/Rotate Amount")]
	public class RotateAmount : StateBehaviour{
		public AttributeGameObject target = new AttributeGameObject();
		public AttributeVector3 amount = Vector3.zero;
		public override void Awake(){
			base.Awake();
			this.amount.Setup("Amount",this);
			this.target.Setup("Target",this);
		}
		public override void Use(){
			base.Use();
			Vector3 amount = this.amount;
			amount *= this.GetTimeOffset();
			foreach(GameObject target in this.target){
				target.transform.localEulerAngles += amount;
			}
		}
	}
}