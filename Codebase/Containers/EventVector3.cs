using Zios;
using System;
using UnityEngine;
[Serializable]
public class EventVector3{
	public Vector3 value;
	public EventVector3(Vector3 value){this.value = value;}
	public static implicit operator EventVector3(Vector3 current){
		return new EventVector3(current);
	}
	public static implicit operator Vector3(EventVector3 current){
		return current.value;
	}
	public void Setup(MonoBehaviour script,string eventName){
		EventUtility.AddGet(script,"Get*"+eventName,this.OnGet);
		EventUtility.AddSet(script,"Set*"+eventName,this.Set);
	}
	public void Set(Vector3 value){this.value = value;}
	public object OnGet(){return this.value;}
	public Vector3 Get(){return this.value;}
	public float GetX(){return this.value.x;}
	public float GetY(){return this.value.y;}
	public float GetZ(){return this.value.z;}
}
