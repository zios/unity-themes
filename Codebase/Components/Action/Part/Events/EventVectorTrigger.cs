using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger (Vector)")]
public class EventVectorTrigger : EventTrigger{
	public Vector2 parameter;
	public bool scaleByIntensity;
	public override void CallEvent(){
		Vector2 value = this.parameter;
		if(this.scaleByIntensity){value *= this.action.intensity;}
		this.eventTarget.Call(this.eventName,value);
	}
}