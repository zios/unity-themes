using UnityEngine;
namespace Zios.Attributes.Deprecated.FollowPoint{
	using Zios.Attributes.Deprecated;
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.State;
	using Zios.Unity.Extensions;
	using Zios.Unity.Extensions.Convert;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Move/Follow Point")]
	public class FollowPoint : StateBehaviour{
		public AttributeGameObject target = new AttributeGameObject();
		public AttributeVector3 goal = Vector3.zero;
		public LerpVector3 position = new LerpVector3();
		public AttributeVector3 orbit = Vector3.zero;
		public override void Awake(){
			base.Awake();
			this.target.Setup("Target",this);
			this.goal.Setup("Goal",this);
			this.position.Setup("Follow",this);
			this.orbit.Setup("Follow Orbit",this);
			this.warnings.AddNew("Deprecated. Consider using formula-based AttributeTransition with ExposeTransform components.");
		}
		public override void Use(){
			Vector3 orbit = this.orbit.Get().ScaleBy(new Vector3(1,-1,1));
			Vector3 end = orbit.ToRotation()*Vector3.zero + this.goal.Get();
			foreach(GameObject target in this.target){
				target.transform.position = this.position.Step(target.transform.position,end);
			}
			base.Use();
		}
	}
}