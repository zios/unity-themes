using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Action End")]
public class ActionEnd: ActionPart{
	public override void Awake(){
		this.DefaultAlias("@End");
		this.DefaultRequirable(false);
		this.DefaultRate("Update");
		this.DefaultPriority(20);
		base.Awake();
	}
	public override void Use(){
		if(this.action.inUse){
			this.action.End();
		}
		base.Use();
	}
}
