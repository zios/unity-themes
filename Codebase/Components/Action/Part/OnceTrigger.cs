using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Once Trigger")]
public class OnceTrigger : ActionPart{
	private bool triggered;
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
