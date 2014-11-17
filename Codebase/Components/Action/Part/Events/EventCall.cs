using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Call")]
public class EventCall : ActionPart{
	public EventTarget target = new EventTarget();
	public override void Awake(){
		base.Awake();
		this.target.Setup("Event",this);
		this.target.mode = EventMode.Listeners;
	}
	public override void Use(){
		this.target.Call();
		base.Use();
	}
}
