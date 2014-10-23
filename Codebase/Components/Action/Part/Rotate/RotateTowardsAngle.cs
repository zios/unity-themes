using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Rotate Towards (Angle)")]
public class RotateTowardsAngle : ActionPart{
	public Vector3 eulerAngle;
	public bool scaleByIntensity;
	public Target target = new Target();
	public ClerpVector3 rotation = new ClerpVector3();
	public override void OnValidate(){
		this.DefaultRate("FixedUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
		this.rotation.Setup(this,"RotateTowards",true);
	}
	public override void Use(){
		Transform transform = this.target.Get().transform;
		Vector3 current = transform.localEulerAngles;
		Vector3 eulerAngle = this.scaleByIntensity ? this.eulerAngle * this.action.intensity : this.eulerAngle;
		transform.localEulerAngles = this.rotation.Step(current,eulerAngle);
		base.Use();
	}
}
