using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Call")]
public class EventCall : ActionPart{
	public string eventName;
	public Target target = new Target();
	public override void OnValidate(){
		base.OnValidate();
		this.target.Setup("Target",this);
	}
	public override void Use(){
		this.target.Call(this.eventName);
		base.Use();
	}
}
