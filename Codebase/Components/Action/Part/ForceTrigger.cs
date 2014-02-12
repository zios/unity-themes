using Zios;
using UnityEngine;
public enum ForceType{Absolute,Relative}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Force Trigger")]
public class ForceTrigger : ActionPart{
	public ForceType type;
	public Vector3 amount;
	public override void Use(){
		Vector3 amount = this.action.intensity * this.amount;
		this.gameObject.Call("AddForce",amount);
	}
}
