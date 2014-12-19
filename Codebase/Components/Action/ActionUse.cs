using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Action Use")]
public class ActionUse : ActionPart{
	public override void Awake(){
		this.DefaultAlias("@Use");
		base.Awake();
	}
	public override void Use(){
		this.action.ready.Set(true);
		base.Use();
	}
	public override void End(){
		this.action.ready.Set(false);
		this.action.End();
		base.End();
	}
}
