using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Action Use State")]
public class ActionUseState : ActionPart{
	public override void OnValidate(){
		this.DefaultPriority(10);
		this.DefaultAlias("@Use");
		base.OnValidate();
	}
	public void Start(){
		this.SetupEvents(this);
	}
	public override void Use(){
		if(!this.inUse){
			this.action.ready = true;
			base.Use();
		}
	}
	public override void OnActionEnd(){
		this.action.ready = false;
		base.End();
	}
	public override void End(){
		bool canEnd = !this.action.inUse;
		if(this.action.persist){canEnd = false;}
		if(canEnd){
			this.OnActionEnd();
		}
	}
}