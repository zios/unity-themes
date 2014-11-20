using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Action State (Freeze)")]
public class ActionStateFreeze : ActionPart{
	public AttributeBool freezeOnUse = false;
	public AttributeBool freezeOnEnd = false;
	public override void Awake(){
		base.Awake();
		this.freezeOnUse.Setup("Freeze On Use",this);
		this.freezeOnEnd.Setup("Freeze On End",this);
	}
	public override void Use(){
		if(this.action.inUse && this.freezeOnEnd){return;}
		base.Use();
	}
	public override void End(){
		if(this.action.inUse && this.freezeOnUse){return;}
		base.End();
	}
}
