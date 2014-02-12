using Zios;
using UnityEngine;
public enum MoveType{Absolute,Relative}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Move Trigger")]
public class MoveTrigger : ActionPart{
	public MoveType type;
	public Vector3 amount;
	public bool scaleByIntensity;
	public void OnValidate(){this.DefaultPriority(15);}
	public override void Use(){
		base.Use();
		Vector3 amount = this.amount;
		if(this.scaleByIntensity){amount *= this.action.intensity;}
		this.action.owner.Call("AddMove",amount);
	}
}