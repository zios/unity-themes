using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Rotate/Towards/Rotate Towards Direction")]
public class RotateTowardsDirection : ActionPart{
	public AttributeGameObject source = new AttributeGameObject();
	public AttributeVector3 target = Vector3.zero;
	public ListBool lerpAxes = new ListBool(){true,true,true};
	public LerpQuaternion rotation = new LerpQuaternion();
	public override void Awake(){
		base.Awake();
		this.DefaultRate("LateUpdate");
		this.source.Setup("Source",this);
		this.target.Setup("Target Direction",this);
		this.rotation.Setup("Rotate Direction",this);
		this.rotation.isAngle.showInEditor = false;
	}
	public override void Use(){
		GameObject source = this.source.Get();
		Vector3 target = this.target.Get();
		if(!target.IsNull() && !source.IsNull()){
			Vector3 angle = source.transform.eulerAngles;
			Quaternion current = source.transform.rotation;
			if(target != Vector3.zero){
				source.transform.rotation = Quaternion.LookRotation(target);
			}
			Quaternion goal = source.transform.rotation;
			source.transform.rotation = this.rotation.Step(current,goal);
			if(this.lerpAxes[1]){angle.x = source.transform.eulerAngles.x;}
			if(this.lerpAxes[0]){angle.y = source.transform.eulerAngles.y;}
			if(this.lerpAxes[2]){angle.z = source.transform.eulerAngles.z;}
			source.transform.eulerAngles = angle;
			base.Use();
		}
	}
}
