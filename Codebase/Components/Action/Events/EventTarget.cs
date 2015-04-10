using Zios;
using System;
using UnityEngine;
namespace Zios{
    public enum EventMode{Listeners,Callers}
    [Serializable]
    public class EventTarget{
	    public AttributeString name = "";
	    public Target target = new Target();
	    public EventMode mode = EventMode.Listeners;
	    public void Setup(string name,Component component){
		    this.name.Setup(name+"/Name",component);
		    this.target.Setup(name+"/Target",component,"");
	    }
	    public void SetupCatch(Method method){
		    GameObject target = this.target.Get();
		    if(!this.name.IsEmpty() && !target.IsNull()){
			    Events.AddScope(this.name,method,target);
		    }
	    }
	    public void Call(){
		    GameObject target = this.target.Get();
			target.CallEvent(this.name);
	    }
    }
}