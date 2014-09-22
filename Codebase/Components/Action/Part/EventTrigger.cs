using Zios;
using UnityEngine;
public enum ValueType{String,Int,Float,Bool}
public enum EventFrequency{Once,Always}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger")]
public class EventTrigger : ActionPart{
	public string eventName;
	public EventFrequency eventFrequency;
	public GameObject eventTarget;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		GameObject target = this.eventTarget == null ? this.action.owner : this.eventTarget;
		if(this.eventFrequency == EventFrequency.Once){
			if(!this.inUse){
				target.Call(this.eventName);
			}
		}
		else{
			target.Call(this.eventName);
		}
		base.Use();
	}
}