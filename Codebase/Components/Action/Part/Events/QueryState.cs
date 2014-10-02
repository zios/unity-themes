using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Query State")]
public class QueryState : ActionPart{
	public string queryName;
	public Target queryTarget;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public void Start(){
		this.queryTarget.AddSpecial("{Owner}",this.action.owner);
	}
	public override void Use(){
		bool state;
		if(this.queryTarget.direct == null){
			state = (bool)Events.Query(this.queryName);
			this.Toggle(state);
			return;
		}
		state = (bool)this.queryTarget.Query(this.queryName);
		this.Toggle(state);
	}
}