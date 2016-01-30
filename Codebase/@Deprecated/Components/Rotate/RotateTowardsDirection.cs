using UnityEngine;
namespace Zios.Actions.RotateComponents{
	using Containers.Math;
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Rotate/Rotate Towards Direction")]
	public class RotateTowardsDirection : StateMonoBehaviour{
		public AttributeGameObject source = new AttributeGameObject();
		public AttributeVector3 goal = Vector3.zero;
		public ListBool lerpAxes = new ListBool(){true,true,true};
		public LerpQuaternion rotation = new LerpQuaternion();
		public override void Awake(){
			base.Awake();
			this.source.Setup("Source",this);
			this.goal.Setup("Goal Direction",this);
			this.rotation.Setup("Rotate Direction",this);
			this.rotation.isResetOnChange.Set(false);
			this.rotation.isResetOnChange.showInEditor = false;
			this.rotation.isAngle.showInEditor = false;
		}
		public override void Use(){
			Vector3 goal = this.goal.Get();
			foreach(GameObject source in this.source){
				Transform transform = source.transform;
				Vector3 angle = transform.eulerAngles;
				Quaternion current = transform.rotation;
				if(goal != Vector3.zero){
					transform.rotation = Quaternion.LookRotation(goal);
				}
				transform.rotation = this.rotation.Step(current,transform.rotation);
				if(this.lerpAxes[1]){angle.x = transform.eulerAngles.x;}
				if(this.lerpAxes[0]){angle.y = transform.eulerAngles.y;}
				if(this.lerpAxes[2]){angle.z = transform.eulerAngles.z;}
				transform.eulerAngles = angle;
				base.Use();
			}
		}
	}
}