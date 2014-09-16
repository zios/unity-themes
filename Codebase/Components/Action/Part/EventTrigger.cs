using Zios;
using UnityEngine;
public enum ValueType{String,Int,Float,Bool}
public enum EventFrequency{Once,Always}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger")]
public class EventTrigger : ActionPart{
	public string eventName;
	public EventFrequency eventFrequency;
	public void OnValidate(){this.DefaultPriority(15);}
	public override void Use(){
		if(this.eventFrequency == EventFrequency.Once){
			if(!this.inUse){
				this.action.owner.Call(this.eventName);
			}
		}
		else{
			this.action.owner.Call(this.eventName);
		}
		base.Use();
	}
}