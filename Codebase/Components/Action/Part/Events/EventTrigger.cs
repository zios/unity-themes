using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger")]
public class EventTrigger : ActionPart{
	public EventSetTarget eventTarget = new EventSetTarget();
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.eventTarget.AddSpecial("[Owner]",this.action.owner);
		this.eventTarget.AddSpecial("[Action]",this.action.gameObject);
		this.eventTarget.DefaultSearch("[Owner]");
	}
	public override void Use(){
		this.eventTarget.Call();
		base.Use();
	}
}