using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Call")]
public class EventCall : ActionPart{
	public EventCallTarget target = new EventCallTarget();
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
	}
	public override void Use(){
		this.target.Call();
		base.Use();
	}
}