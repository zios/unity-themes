using UnityEngine;
namespace Zios.Actions.RotateComponents{
	using Containers.Math;
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Rotate/Rotate Towards Point")]
	public class RotateTowardsPoint : StateMonoBehaviour{
		public AttributeGameObject source = new AttributeGameObject();
		public AttributeVector3 goal = Vector3.zero;
		public ListBool lerpAxes = new ListBool(){true,true,true};
		public LerpQuaternion rotation = new LerpQuaternion();
		public override void Awake(){
			base.Awake();
			this.source.Setup("Source",this);
			this.goal.Setup("Goal Point",this);
			this.rotation.Setup("Rotation Point",this);
			this.rotation.isResetOnChange.Set(false);
			this.rotation.isResetOnChange.showInEditor = false;
			this.rotation.isAngle.showInEditor = false;
		}
		public override void Use(){
			Vector3 goal = this.goal.Get();
			foreach(GameObject source in this.source){
				Vector3 angle = source.transform.eulerAngles;
				Quaternion current = source.transform.rotation;
				source.transform.LookAt(goal);
				source.transform.rotation = this.rotation.Step(current,source.transform.rotation);
				if(this.lerpAxes[1]){angle.x = source.transform.eulerAngles.x;}
				if(this.lerpAxes[0]){angle.y = source.transform.eulerAngles.y;}
				if(this.lerpAxes[2]){angle.z = source.transform.eulerAngles.z;}
				source.transform.eulerAngles = angle;
			}
			base.Use();
		}
	}
}