using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Action End")]
public class ActionEnd: ActionPart{
	public override void OnValidate(){
		base.OnValidate();
		this.DefaultRequirable(false);
		this.DefaultRate("Update");
		this.DefaultPriority(20);
		this.DefaultAlias("@End");
	}
	public override void Use(){
		if(this.action.inUse){
			this.action.End();
		}
		base.Use();
	}
}