using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Listen")]
public class EventListen : ActionPart{
	public string eventName;
	public Target target = new Target();
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.AddSpecial("[Owner]",this.action.owner);
		this.target.AddSpecial("[Action]",this.action.gameObject);
		this.target.DefaultSearch("[Action]");
	}
	public void Start(){
		Events.AddTarget(this.eventName,this.Catch,this.target.Get());
	}
	public override void Use(){}
	public void Catch(){
		base.Use();
	}
}