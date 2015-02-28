using Zios;
using UnityEngine;
public enum MoveType{Absolute,Relative}
[AddComponentMenu("Zios/Component/Action/Move/Add Move")]
public class AddMove : ActionLink{
	public MoveType type;
	public AttributeVector3 amount = Vector3.zero;
	public AttributeGameObject target = new AttributeGameObject();
	public override void Awake(){
		base.Awake();
		this.AddDependent<ColliderController>(this.target);
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
		this.target.Get().Call("Add Move",amount);
	}
}
