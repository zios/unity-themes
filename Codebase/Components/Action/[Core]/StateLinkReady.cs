using Zios;
using UnityEngine;
[RequireComponent(typeof(StateLink))][AddComponentMenu("Zios/Component/Action/State Link Ready")]
public class StateLinkReady : ActionLink{
	public override void Awake(){
		this.DefaultAlias("@Ready");
		base.Awake();
	}
	public override void Use(){
		this.stateLink.ready.Set(true);
		base.Use();
	}
	public override void End(){
		this.stateLink.ready.Set(false);
		this.stateLink.End();
		base.End();
	}
}
