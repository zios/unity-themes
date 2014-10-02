using Zios;
using UnityEngine;
public enum ForceType{Absolute,Relative}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Force Trigger")]
public class ForceTrigger : ActionPart{
	public ForceType type;
	public Vector3 amount;
	public bool scaleByIntensity;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		base.Use();
		Vector3 amount = this.amount;
		if(this.type == ForceType.Relative){
			amount = this.action.owner.transform.right * this.amount.x;
			amount += this.action.owner.transform.up * this.amount.y;
			amount += this.action.owner.transform.forward * this.amount.z;
		}
		if(this.scaleByIntensity){amount *= this.action.intensity;}
		this.action.owner.Call("AddForce",amount);
	}
}