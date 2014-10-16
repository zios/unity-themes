using Zios;
using System;
using UnityEngine;
[Serializable]
public class EventFloat{
	public float value;
	public EventFloat(float value){this.value = value;}
	public static implicit operator EventFloat(float current){
		return new EventFloat(current);
	}
	public static implicit operator float(EventFloat current){
		return current.value;
	}
	public void Setup(MonoBehaviour script,string eventName){
		EventUtility.AddGet(script,"Get*"+eventName,this.Get);
		EventUtility.AddSet(script,"Set*"+eventName,this.Set);
	}
	public object Get(){return this.value;}
	public void Set(float value){this.value = value;}
}
