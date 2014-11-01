using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Action State (Freeze)")]
public class ActionStateFreeze : ActionPart{
	public AttributeBool freezeOnUse;
	public AttributeBool freezeOnEnd;
	public override void OnValidate(){
		base.OnValidate();
		this.freezeOnUse.Setup("FreezeOnUse",this);
		this.freezeOnEnd.Setup("FreezeOnEnd",this);
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