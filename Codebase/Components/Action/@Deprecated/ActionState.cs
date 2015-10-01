using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Action State")]
public class ActionState : StateMonoBehaviour{
	public override void Awake(){
		base.Awake();
		string warning = "This component has been deprecated and likely should not be used.";
		if(!this.dependents.Exists(x=>x.message==warning)){
			this.dependents.AddNew().message = warning;
		}
	}
}
