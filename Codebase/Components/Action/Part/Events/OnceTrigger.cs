using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Once Trigger")]
public class OnceTrigger : ActionPart{
	private bool triggered;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public void Start(){
		Events.Add("ActionEnd",this.OnActionEnd);
	}
	public override void Use(){
		if(!this.triggered){
			this.triggered = true;
			base.Use();
			return;
		}
		if(this.inUse){
			base.End();
		}
	}
	public void OnActionEnd(){
		base.End();
		this.triggered = false;
	}
}