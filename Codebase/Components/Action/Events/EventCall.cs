using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Event/Event Call")]
public class EventCall : ActionLink{
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
