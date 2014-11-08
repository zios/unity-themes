using Zios;
using System;
using UnityEngine;
[Serializable]
public class EventTarget{
	public string name;
	public Target target = new Target();
	public void Setup(string name,params MonoBehaviour[] scripts){
		this.target.Setup(name,scripts);
		this.target.DefaultSearch("[Action]");
	}
	public void SetupCatch(Method method){
		if(!this.name.IsEmpty() && this.target.direct != null){
			Events.AddScope(this.name,method,this.target.direct);
		}
	}
	public void Call(){
		this.target.Call(this.name);
	}
}