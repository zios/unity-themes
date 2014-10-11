using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Action End Trigger")]
public class ActionEndTrigger : ActionPart{
	public override void OnValidate(){
		this.rate = ActionRate.Update;
		this.DefaultRequirable(false);
		this.DefaultPriority(20);
		this.DefaultAlias("@End");
		base.OnValidate();
	}
	public override void Use(){
		if(this.action.inUse){
			this.action.End();
		}
		base.Use();
	}
}