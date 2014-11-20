using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Once Trigger")]
public class OnceTrigger : ActionPart{
	private bool triggered;
	public override void Awake(){
		Events.Add("Action End",this.OnActionEnd);
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
