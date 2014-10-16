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
	public void Update(MonoBehaviour script){this.scope.Setup(script);}
	public void Setup(MonoBehaviour script,string eventName=""){this.scope.Setup(script,eventName);}
	public object Get(string name){
		this.scope.Prepare();
		string scopeName = !this.scope.direct ? "Events" : this.scope.Get().name;
		if(name.IsEmpty()){
			Debug.LogWarning("EventQuery : Empty get events string."); 
			return null;
		}
		object value = !this.scope.direct ? Events.Query(name) : this.scope.Query(name);
		if(value == null){
			Debug.LogWarning("EventQuery : (" + scopeName + " -- " + name + ") returned null.");
		}
		return value;
	}
	public void Set(string name,object value){
		this.scope.Prepare();
		if(this.scope.direct == null){
			Events.Call(name,value);
			return;
		}
		this.scope.Call(name,value);
	}
	public void Call(string name){
		this.scope.Prepare();
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
public class EventCallTarget : EventTarget{
	public string callEvent;
	public void Call(){this.Call(this.callEvent);}
}
[Serializable]
public class EventGetTarget : EventTarget{
	public string getEvent;
	public object Get(){return this.Get(this.getEvent);}
}
[Serializable]
public class EventSetTarget : EventTarget{
	public string setEvent;
	public string parameter;
	public ValueType parameterType;
	public void Set(){
		ValueType type = this.parameterType;
		string name = this.setEvent;
		string value = this.parameter;
		if(type == ValueType.String){this.Set(name,value);}
		if(type == ValueType.Int){this.Set(name,value.ToInt());}
		if(type == ValueType.Float){this.Set(name,value.ToFloat());}
		if(type == ValueType.Bool){this.Set(name,value.ToBool());}
	}
	public void Set(object value){this.Set(this.setEvent,value);}
}
public enum ValueType{String,Int,Float,Bool}