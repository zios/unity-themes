using UnityEngine;
namespace Zios.Attributes.Deprecated.Rotate{
	using Zios.Attributes.Supports;
	[AddComponentMenu("Zios/Component/Action/Rotate/Rotate Towards Target")]
	public class RotateTowardsTarget : RotateTowardsPoint{
		public AttributeGameObject target = new AttributeGameObject();
		public override void Awake(){
			base.Awake();
			this.target.Setup("Target",this);
			this.goal.showInEditor = false;
		}
		public override void Use(){
			Vector3 goalPosition = this.target.Get().transform.position;
			this.goal.Set(goalPosition);
			base.Use();
		}
	}
}