using Zios;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Action Ready")]
    public class ActionReady : ActionLink{
	    public override void Awake(){
		    this.DefaultAlias("@Ready");
		    base.Awake();
		    this.AddDependent<StateLink>();
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
}