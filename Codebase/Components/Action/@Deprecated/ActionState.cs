using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Action State")]
public class ActionState : ActionLink{
	public override void Awake(){
		base.Awake();
		this.warnings["This component has been deprecated and likely should not be used."] = ()=>{};
	}
}
