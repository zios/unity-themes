using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Action Use")]
public class ActionUse : ActionPart{
	public override void Awake(){
		this.DefaultAlias("@Use");
		this.DefaultPriority(10);
		base.Awake();
		Events.Add("Action End",this.OnActionEnd);
	}
	public override void Use(){
		if(!this.inUse){
			this.action.ready = true;
			base.Use();
		}
	}
	public void OnActionEnd(){
		this.action.ready = false;
		base.End();
	}
	public override void End(){
		if(!this.action.inUse){
			this.OnActionEnd();
		}
	}
}
