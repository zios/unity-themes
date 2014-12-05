using Zios;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Rotate/Towards/Rotate Towards Point")]
public class RotateTowardsPoint : ActionPart{
	public AttributeGameObject target = new AttributeGameObject();
	public AttributeVector3 goal = Vector3.zero;
	public LerpQuaternion angles = new LerpQuaternion();
	public ListBool lerpAxes = new ListBool(){true,true,true};
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.goal.Setup("Goal",this);
		this.angles.Setup("Angles",this);
		this.angles.isResetOnChange.Set(false);
		this.angles.isResetOnChange.showInEditor = false;
		this.angles.isAngle.showInEditor = false;
	}
	public override void End(){
		this.angles.Reset();
		base.End();
	}
	public override void Use(){
		GameObject target = this.target.Get();
		Vector3 goal = this.goal.Get();
		if(!goal.IsNull() && !target.IsNull()){
			Vector3 angle = target.transform.eulerAngles;
			Quaternion current = target.transform.rotation;
			target.transform.LookAt(goal);
			target.transform.rotation = this.angles.Step(current,target.transform.rotation);
			if(this.lerpAxes[1]){angle.x = target.transform.eulerAngles.x;}
			if(this.lerpAxes[0]){angle.y = target.transform.eulerAngles.y;}
			if(this.lerpAxes[2]){angle.z = target.transform.eulerAngles.z;}
			target.transform.eulerAngles = angle;
			base.Use();
		}
	}
}
