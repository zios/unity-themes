using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Call")]
public class EventCall : ActionPart{
	public EventTarget target = new EventTarget();
	public override void Start(){
		base.Start();
		this.target.Setup("Event",this);
	}
	public override void Use(){
		this.target.Call();
		base.Use();
	}
}