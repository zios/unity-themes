using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Rotate Amount")]
public class RotateAmount : ActionPart{
	public Vector3 amount;
	public bool scaleByIntensity;
	public Target target = new Target();
	public override void OnValidate(){
		this.DefaultRate("FixedUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
	}
	public override void Use(){
		base.Use();
		Transform target = this.target.Get().transform;
		Vector3 amount = this.amount * Time.fixedDeltaTime;
		if(this.scaleByIntensity){amount *= this.action.intensity;}
		target.localEulerAngles += amount;
	}
}
