using Zios;
using UnityEngine;
public enum MoveType{Absolute,Relative}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Move Trigger")]
public class MoveTrigger : ActionPart{
	public MoveType type;
	public Vector3 amount;
	public bool scaleByIntensity;
	public override void OnValidate(){
		this.DefaultRate("FixedUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		base.Use();
		Vector3 amount = this.amount;
		if(this.type == MoveType.Relative){
			amount = this.action.owner.transform.right * this.amount.x;
			amount += this.action.owner.transform.up * this.amount.y;
			amount += this.action.owner.transform.forward * this.amount.z;
		}
		if(this.scaleByIntensity){amount *= this.action.intensity;}
		this.action.owner.Call("AddMove",amount);
	}
}