using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Action End")]
public class ActionEnd: ActionPart{
	public override void Awake(){
		base.Awake();
		this.DefaultAlias("@End");
		this.DefaultRequirable(false);
	}
	public override void Use(){
		if(this.action.inUse){
			this.action.End();
		}
		base.Use();
	}
}
