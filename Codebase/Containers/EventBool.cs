using Zios;
using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class EventBool{
	public bool value;
	public EventBool(bool value){this.value = value;}
	public static implicit operator EventBool(bool current){
		return new EventBool(current);
	}
	public static implicit operator bool(EventBool current){
		return current.value;
	}
	public void Setup(MonoBehaviour script,string eventName){
		EventUtility.AddGet(script,"Get*"+eventName,this.Get);
		EventUtility.AddSet(script,"Set*"+eventName,this.Set);
	}
	public object Get(){return this.value;}
	public void Set(bool value){this.value = value;}
}