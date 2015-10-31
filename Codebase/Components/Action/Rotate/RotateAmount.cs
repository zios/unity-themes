using Zios;
using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Rotate/Rotate Amount")]
	public class RotateAmount : StateMonoBehaviour{
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