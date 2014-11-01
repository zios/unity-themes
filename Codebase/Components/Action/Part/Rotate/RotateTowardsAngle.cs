using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Rotate Towards (Angle)")]
public class RotateTowardsAngle : ActionPart{
	public AttributeVector3 eulerAngle;
	public Target target = new Target();
	public ClerpVector3 rotation = new ClerpVector3();
	public override void OnValidate(){
		base.OnValidate();
		this.DefaultRate("FixedUpdate");
		this.target.Setup("Target",this);
		this.rotation.Setup("RotateTowards",this);
		this.rotation.isAngle = true;
	}
	public override void Use(){
		Transform transform = this.target.Get().transform;
		Vector3 current = transform.localEulerAngles;
		transform.localEulerAngles = this.rotation.Step(current,this.eulerAngle);
		base.Use();
	}
}
