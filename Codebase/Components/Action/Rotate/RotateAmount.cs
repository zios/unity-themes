using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Rotate/Rotate Amount")]
public class RotateAmount : ActionLink{
	public AttributeVector3 amount = Vector3.zero;
	public AttributeGameObject target = new AttributeGameObject();
	public override void Awake(){
		base.Awake();
		this.amount.Setup("Amount",this);
		this.target.Setup("Target",this);
	}
	public override void Use(){
		base.Use();
		Transform target = this.target.Get().transform;
		Vector3 amount = this.amount;
		amount *= this.deltaTime;
		target.localEulerAngles += amount;
	}
}
