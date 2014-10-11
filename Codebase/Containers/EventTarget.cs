using System;
using System.Collections.Generic;
using UnityEngine;
using Zios;
[Serializable]
public class EventTarget{
	public Target scope = new Target();
	public void AddSpecial(string name,GameObject target){this.scope.AddSpecial(name,target);}
	public void DefaultSearch(string target){this.scope.DefaultSearch(target);}
	public void DefaultTarget(GameObject target){this.scope.DefaultTarget(target);}
	public object Get(string name){
		this.scope.Setup();
		if(this.scope.direct == null){return Events.Query(name);}
		return this.scope.Query(name);
	}
	public void Set(string name,object value){
		this.scope.Setup();
		if(this.scope.direct == null){
			Events.Call(name,value);
			return;
		}
		this.scope.Call(name,value);
	}
	public void Call(string name){
		this.scope.Setup();
		if(this.scope.direct == null){
			Events.Call(name);
			return;
		}
		this.scope.Call(name);
	}
}
[Serializable]
public class EventManageTarget : EventTarget{
	public string getEvent;
	public string setEvent;
	public object Get(){return this.Get(this.getEvent);}
	public void Set(object value){this.Set(this.setEvent,value);}
	public void Call(){this.Set(this.setEvent);}
}
[Serializable]
public class EventGetTarget : EventTarget{
	public string getEvent;
	public object Get(){return this.Get(this.getEvent);}
}
[Serializable]
public class EventSetTarget : EventTarget{
	public string setEvent;
	public void Set(object value){this.Set(this.setEvent,value);}
	public void Call(){this.Set(this.setEvent);}
}