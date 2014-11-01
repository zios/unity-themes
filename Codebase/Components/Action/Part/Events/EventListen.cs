using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Listen")]
public class EventListen : ActionPart{
	public string eventName;
	public Target target = new Target();
	public override void OnValidate(){
		base.OnValidate();
		this.target.Setup("Target",this);
		this.target.DefaultSearch("[Action]");
	}
	public void Start(){
		Events.AddScope(this.eventName,this.Catch,this.target.Get().gameObject);
	}
	public override void Use(){}
	public void Catch(){
		base.Use();
	}
}
