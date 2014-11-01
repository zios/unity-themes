using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Clamp (Transform)")]
public class ClampTransform : ActionPart{
	public Target target = new Target();
	public ClerpVector3 position = new ClerpVector3();
	public ClerpVector3 rotation = new ClerpVector3();
	public ClerpVector3 scale = new ClerpVector3();
	public override void OnValidate(){
		base.OnValidate();
		this.DefaultRate("LateUpdate");
		this.target.Setup("Target",this);
		this.position.Setup("Position",this);
		this.rotation.Setup("Rotation",this);
		this.rotation.isAngle = true;
		this.scale.Setup("Scale",this);
	}
	public override void Use(){
		Transform target = this.target.Get().transform;
		target.position = this.position.Step(target.position);
		target.localEulerAngles = this.rotation.Step(target.localEulerAngles);
		target.localScale = this.scale.Step(target.localScale);
		base.Use();
	}
}
