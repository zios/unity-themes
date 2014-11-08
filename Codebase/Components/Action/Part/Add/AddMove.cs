using Zios;
using UnityEngine;
public enum MoveType{Absolute,Relative}
[AddComponentMenu("Zios/Component/Action/Part/Add Move")]
public class AddMove : ActionPart{
	public MoveType type;
	public AttributeVector3 amount;
	public Target target = new Target();
	public override void Start(){
		base.Start();
		this.DefaultRate("FixedUpdate");
		this.target.Setup("Target",this);
		this.amount.Setup("Amount",this);
	}
	public override void Use(){
		base.Use();
		Vector3 amount = this.amount;
		Transform transform = this.target.Get().transform;
		if(this.type == MoveType.Relative){
			amount = transform.right * this.amount.x;
			amount += transform.up * this.amount.y;
			amount += transform.forward * this.amount.z;
		}
		this.target.Get().Call("AddMove",amount);
	}
}
