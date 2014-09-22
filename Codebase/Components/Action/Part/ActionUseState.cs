using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Action Use State")]
public class ActionUseState : ActionPart{
	public override void OnValidate(){
		this.DefaultPriority(10);
		this.DefaultAlias("@Use");
		base.OnValidate();
	}
	public override void Use(){
		this.action.ready = true;
		base.Use();
	}
	public override void End(){
		this.action.ready = false;
		base.End();
	}
}