using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Listen")]
public class EventListen : ActionPart{
	public string eventName;
	public Target target = new Target();
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
		this.target.DefaultSearch("[Action]");
	}
	public void Start(){
		this.target.Setup(this);
		Events.AddScope(this.eventName,this.Catch,this.target.Get().gameObject);
	}
	public override void Use(){}
	public void Catch(){
		base.Use();
	}
}
