using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Listen")]
public class EventListen : ActionPart{
	public EventTarget target = new EventTarget();
	public override void Start(){
		base.Start();
		this.target.Setup("Event",this);
		this.target.SetupCatch(this.Catch);
	}
	public override void Use(){}
	public void Catch(){
		base.Use();
	}
}
