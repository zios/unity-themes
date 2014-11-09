using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Rotate Amount")]
public class RotateAmount : ActionPart{
	public AttributeVector3 amount = Vector3.zero;
	public Target target = new Target();
	public override void Start(){
		base.Start();
		this.DefaultRate("FixedUpdate");
		this.target.Setup("Target",this);
	}
	public override void Use(){
		base.Use();
		Transform target = this.target.Get().transform;
		Vector3 amount = this.amount;
		amount *= this.rate == ActionRate.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
		target.localEulerAngles += amount;
	}
}
