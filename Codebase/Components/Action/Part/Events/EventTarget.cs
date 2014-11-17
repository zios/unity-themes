using Zios;
using System;
using UnityEngine;
public enum EventMode{Listeners,Callers}
[Serializable]
public class EventTarget{
	public AttributeString name = "";
	public Target target = new Target();
	public EventMode mode = EventMode.Listeners;
	public void Setup(string name,Component component){
		this.name.Setup(name+"/Name",component);
		this.target.SkipWarning();
		this.target.Setup(name+"/Target",component,"");
	}
	public void SetupCatch(Method method){
		if(!this.name.IsEmpty() && this.target.direct != null){
			Events.AddScope(this.name,method,this.target.direct);
		}
	}
	public void Call(){
		if(Events.HasEvent(name,this.target.direct)){
			this.target.Call(this.name);
		}
	}
}
