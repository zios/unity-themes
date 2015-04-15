using Zios;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Move/Add Force")]
    public class AddForce : AddMove{
	    public override void Awake(){
		    base.Awake();
			this.eventName = "Add Force";
			this.eventNameOnce = "Add Force Raw";
		    this.AddDependent<Force>(this.target);
	    }
    }
}