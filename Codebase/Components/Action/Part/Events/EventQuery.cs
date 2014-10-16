using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Query")]
public class EventQuery : ActionPart{
	public EventGetTarget target = new EventGetTarget();
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
	}
	public override void Use(){
		bool state = (bool)this.target.Get();
		this.Toggle(state);
	}
}