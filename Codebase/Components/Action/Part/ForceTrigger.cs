using Zios;
using UnityEngine;
public enum ForceType{Absolute,Relative}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Force Trigger")]
public class ForceTrigger : ActionPart{
	public ForceType type;
	public Vector3 amount;
	public bool scaleByIntensity;
	public Target target;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.AddSpecial("[Owner]",this.action.owner);
		this.target.AddSpecial("[Action]",this.action.gameObject);
		this.target.DefaultSearch("[Owner]");
	}
	public override void Use(){
		base.Use();
		Vector3 amount = this.amount;
		if(this.type == ForceType.Relative){
			amount = this.target.Get().transform.right * this.amount.x;
			amount += this.target.Get().transform.up * this.amount.y;
			amount += this.target.Get().transform.forward * this.amount.z;
		}
		if(this.scaleByIntensity){amount *= this.action.intensity;}
		this.target.Call("AddForce",amount);
	}
}