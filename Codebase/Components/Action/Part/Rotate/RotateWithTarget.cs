using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Rotate With (Target)")]
public class RotateWithTarget : ActionPart{
	public Target source = new Target();
	public Target target = new Target();
	public LerpVector3 angles = new LerpVector3();
	public override void OnValidate(){
		base.OnValidate();
		this.DefaultRate("LateUpdate");
		this.source.Setup("Source",this);
		this.target.Setup("Target",this);
		this.angles.Setup("Angles",this);
		this.angles.isAngle = true;
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform target = this.target.Get().transform;
		Vector3 current = source.localEulerAngles;
		Vector3 goal = target.localEulerAngles;
		source.localEulerAngles = this.angles.Step(current,goal);
		base.Use();
	}
}
