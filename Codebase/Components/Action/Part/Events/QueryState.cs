using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Query State")]
public class QueryState : ActionPart{
	public EventGetTarget queryTarget = new EventGetTarget();
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.queryTarget.AddSpecial("[Owner]",this.action.owner);
		this.queryTarget.AddSpecial("[Action]",this.action.gameObject);
		this.queryTarget.DefaultSearch("[Owner]");
	}
	public override void Use(){
		bool state = (bool)this.queryTarget.Get();
		this.Toggle(state);
	}
}