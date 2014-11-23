using Zios;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Rotate/Towards/Rotate Towards Point")]
public class RotateTowardsPoint : ActionPart{
	public AttributeGameObject source = new AttributeGameObject();
	public AttributeVector3 target = Vector3.zero;
	public LerpQuaternion angles = new LerpQuaternion();
	public ListBool lerpAxes = new ListBool(){true,true,true};
	public override void Awake(){
		base.Awake();
		this.DefaultRate("LateUpdate");
		this.source.Setup("Source",this);
		this.target.Setup("Target",this);
		this.angles.Setup("Angles",this);
		this.angles.isResetOnChange.Set(false);
	}
	public override void End(){
		this.angles.Reset();
		base.End();
	}
	public override void Use(){
		GameObject source = this.source.Get();
		Vector3 target = this.target.Get();
		if(!target.IsNull() && !source.IsNull()){
			Vector3 angle = source.transform.eulerAngles;
			Quaternion current = source.transform.rotation;
			source.transform.LookAt(target);
			Quaternion goal = source.transform.rotation;
			source.transform.rotation = this.angles.Step(current,goal);
			if(this.lerpAxes[1]){angle.x = source.transform.eulerAngles.x;}
			if(this.lerpAxes[0]){angle.y = source.transform.eulerAngles.y;}
			if(this.lerpAxes[2]){angle.z = source.transform.eulerAngles.z;}
			source.transform.eulerAngles = angle;
			base.Use();
		}
	}
}
