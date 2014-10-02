using Zios;
using System;
using UnityEngine;
public enum ValueType{String,Int,Float,Bool}
public enum EventFrequency{Always,Once}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger")]
public class EventTrigger : ActionPart{
	public string eventName;
	public EventFrequency eventFrequency;
	public Target eventTarget;
	public void Start(){
		this.eventTarget.SetDefault(this.action.owner);
	}
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		if(this.eventFrequency == EventFrequency.Once){
			if(!this.inUse){this.CallEvent();}
		}
		else{this.CallEvent();}
		base.Use();
	}
	public virtual void CallEvent(){
		this.eventTarget.Call(this.eventName);
	}
}