using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger (Vector)")]
public class EventVectorTrigger : ActionPart{
	public string eventName;
	public Vector2 parameter;
	public bool scaleByIntensity;
	public EventFrequency eventFrequency;
	public GameObject eventTarget;
	public override void OnValidate(){
		this.constant = true;
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		base.Use();
		if(this.eventFrequency == EventFrequency.Once){
			if(!this.inUse){this.CallEvent();}
		}
		else{this.CallEvent();}
	}
	public void CallEvent(){
		GameObject target = this.eventTarget == null ? this.action.owner : this.eventTarget;
		Vector2 value = this.parameter;
		if(this.scaleByIntensity){value *= this.action.intensity;}
		target.Call(this.eventName,value);
	}
}