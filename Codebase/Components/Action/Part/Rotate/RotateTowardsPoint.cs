using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Rotate Towards (Point)")]
public class RotateTowardsPoint : ActionPart{
	public Target source = new Target();
	public AttributeVector3 target = Vector3.zero;
	public LerpQuaternion angles = new LerpQuaternion();
	public override void Awake(){
		base.Awake();
		this.DefaultRate("LateUpdate");
		this.source.Setup("Source",this);
		this.target.Setup("Target",this);
		this.angles.Setup("Angles",this);
	}
	public override void End(){
		this.angles.Reset();
		base.End();
	}
	public override void Use(){
		GameObject source = this.source.Get();
		Vector3 target = this.target.Get();
		if(!target.IsNull() && !source.IsNull()){
			Quaternion current = source.transform.rotation;
			source.transform.LookAt(target);
			Quaternion goal = source.transform.rotation;
			source.transform.rotation = this.angles.Step(current,goal);
			base.Use();
		}
	}
}
