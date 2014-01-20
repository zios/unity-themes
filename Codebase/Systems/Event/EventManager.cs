using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
public static class Events{
	private static Dictionary<GameObject,Dictionary<string,object>> objectEvents = new Dictionary<GameObject,Dictionary<string,object>>();
	private static Dictionary<string,List<object>> events = new Dictionary<string,List<object>>();
	public static void Add(string name,Method method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodObject method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodFull method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodString method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodInt method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodFloat method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodBool method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodVector2 method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodVector3 method){Events.Add(name,(object)method);}
	public static void Add(string name,object method){
		object methodTarget = ((Delegate)method).Target;
		if(!Events.events.ContainsKey(name)){
			Events.events[name] = new List<object>();
		}
		if(!Events.events[name].Contains(method)){
			Events.events[name].Add(method);
		}
		if(methodTarget != null){
			Type type = methodTarget.GetType();
			if(type.IsSubclassOf((typeof(MonoBehaviour)))){
				GameObject target = ((MonoBehaviour)methodTarget).gameObject;
				if(!Events.objectEvents.ContainsKey(target)){
					Events.objectEvents[target] = new Dictionary<string,object>();
				}
				Events.objectEvents[target][name] = method;
			}
		}
	}
	public static void Handle(object callback,object[] values){
		object value = values.Length > 0 ? values[0] : null;
		if(values.Length > 1 && callback is MethodFull){
			((MethodFull)callback)(values);
		}
		else if(value == null || callback is Method){
			((Method)callback)();
		}
		else if(value is object && callback is MethodObject){
			((MethodObject)callback)((object)value);
		}
		else if(value is int && callback is MethodInt){
			((MethodInt)callback)((int)value);
		}
		else if(value is float && callback is MethodFloat){
			((MethodFloat)callback)((float)value);
		}
		else if(value is string && callback is MethodString){
			((MethodString)callback)((string)value);
		}
		else if(value is bool && callback is MethodBool){
			((MethodBool)callback)((bool)value);
		}
		else if(value is Vector2 && callback is MethodVector2){
			((MethodVector2)callback)((Vector2)value);
		}
		else if(value is Vector3 && callback is MethodVector3){
			((MethodVector3)callback)((Vector3)value);
		}
	}
	public static void Call(string name,params object[] values){
		if(Events.events.ContainsKey(name)){
			foreach(object callback in Events.events[name]){
				Events.Handle(callback,values);
			}
		}
	}
	public static void Call(GameObject target,string name,object[] values){
		if(Events.objectEvents.ContainsKey(target)){
			if(Events.objectEvents[target].ContainsKey(name)){
				object callback = Events.objectEvents[target][name];
				Events.Handle(callback,values);
			}
		}
	}
	public static void CallChildren(GameObject target,string name,object[] values,bool self=false){
		if(self){Events.Call(target,name,values);}
		Transform[] children = target.GetComponentsInChildren<Transform>();
		foreach(Transform transform in children){
			if(transform.gameObject == target){continue;}
			Events.CallChildren(transform.gameObject,name,values,true);
		}
	}
	public static void CallParents(GameObject target,string name,object[] values,bool self=false){
		if(self){Events.Call(target,name,values);}
		Transform parent = target.transform.parent;
		while(parent != null){
			Events.CallParents(parent.gameObject,name,values,true);
			parent = parent.parent;
		}
	}
	public static void CallFamily(GameObject target,string name,object[] values,bool self=false){
		if(self){Events.Call(target,name,values);}
		Events.CallChildren(target,name,values);
		Events.CallParents(target,name,values);
	}
}
public static class GameObjectEvents{
	public static void Call(this GameObject current,string name,params object[] values){
		Events.Call(current,name,values);
	}
	public static void CallChildren(this GameObject current,string name,bool self=true,params object[] values){
		Events.CallChildren(current,name,values,self);
	}
	public static void CallParents(this GameObject current,string name,bool self=true,params object[] values){
		Events.CallParents(current,name,values,self);
	}
	public static void CallFamily(this GameObject current,string name,bool self=true,params object[] values){
		Events.CallFamily(current,name,values,self);
	}
}