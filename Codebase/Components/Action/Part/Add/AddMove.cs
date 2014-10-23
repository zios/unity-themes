using Zios;
using UnityEngine;
public enum MoveType{Absolute,Relative}
[AddComponentMenu("Zios/Component/Action/Part/Add Move")]
public class AddMove : ActionPart{
	public MoveType type;
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
		Vector3 amount = this.amount;
		Transform transform = this.target.Get().transform;
		if(this.type == MoveType.Relative){
			amount = transform.right * this.amount.x;
			amount += transform.up * this.amount.y;
			amount += transform.forward * this.amount.z;
		}
		if(this.scaleByIntensity){amount *= this.action.intensity;}
		this.target.Get().Call("AddMove",amount);
	}
}
