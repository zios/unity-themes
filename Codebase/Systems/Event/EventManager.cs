using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
public static class Events{
	private static Dictionary<GameObject,Dictionary<string,List<object>>> objectEvents = new Dictionary<GameObject,Dictionary<string,List<object>>>();
	private static Dictionary<string,List<object>> events = new Dictionary<string,List<object>>();
	static Events(){
		Events.objectEvents.Clear();
		Events.events.Clear();
	}
	public static void AddGet(string name,MethodStringReturn method){Events.Add(name,(object)method);}
	public static void AddGet(string name,MethodReturn method){Events.Add(name,(object)method);}
	public static void Add(string name,Method method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodObject method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodFull method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodString method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodInt method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodFloat method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodBool method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodVector2 method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodVector3 method){Events.Add(name,(object)method);}
	public static void AddGetScope(string name,MethodReturn method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,Method method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,MethodObject method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,MethodFull method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,MethodString method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,MethodInt method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,MethodFloat method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,MethodBool method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,MethodVector2 method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,MethodVector3 method,params GameObject[] targets){Events.AddScope(name,(object)method,targets);}
	public static void AddScope(string name,object method,GameObject[] targets,bool addMethod=true){
		name = name.ToLower();
		if(addMethod){Events.Add(name,method);}
		foreach(GameObject target in targets){
			if(!Events.objectEvents.ContainsKey(target)){
				Events.objectEvents[target] = new Dictionary<string,List<object>>();
			}
			if(!Events.objectEvents[target].ContainsKey(name)){
				Events.objectEvents[target][name] = new List<object>();
			}
			if(!Events.objectEvents[target][name].Contains(method)){
				Events.objectEvents[target][name].Add(method);
			}
		}
	}
	public static void Add(string name,object method){
		name = name.ToLower();
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
				Events.AddScope(name,method,new GameObject[]{target},false);
			}
		}
	}
	public static bool IsGetMethod(object callback){
		if(callback is MethodReturn){return true;}
		if(callback is MethodStringReturn){return true;}
		return false;
	}
	public static void Handle(object callback,object[] values){
		object value = values.Length > 0 ? values[0] : null;
		if(callback is MethodFull){
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
	public static object HandleGet(object callback,object[] values){
		object value = values.Length > 0 ? values[0] : null;
		if(callback is MethodReturn){
			return ((MethodReturn)callback)();
		}
		else if(value is object && callback is MethodObjectReturn){
			return ((MethodObjectReturn)callback)((object)value);
		}
		else if(value is int && callback is MethodIntReturn){
			return ((MethodIntReturn)callback)((int)value);
		}
		else if(value is float && callback is MethodFloatReturn){
			return ((MethodFloatReturn)callback)((float)value);
		}
		else if(value is string && callback is MethodStringReturn){
			return ((MethodStringReturn)callback)((string)value);
		}
		else if(value is bool && callback is MethodBoolReturn){
			return ((MethodBoolReturn)callback)((bool)value);
		}
		else if(value is Vector2 && callback is MethodVector2Return){
			return ((MethodVector2Return)callback)((Vector2)value);
		}
		else if(value is Vector3 && callback is MethodVector3Return){
			return ((MethodVector3Return)callback)((Vector3)value);
		}
		return null;
	}
	private static void CheckSpecial(GameObject target,string name,object[] values){
		string prefix = Events.GetSpecialPrefix(target,name);
		string suffix = Events.GetSpecialSuffix(target,name);
		object value = values.Length > 0 ? values[0] : null;
		object setMethod = Events.GetSpecialCallback(target,name,"set");
		object getMethod = Events.GetSpecialCallback(target,name,"get");
		if(getMethod == null || setMethod == null || value == null){return;}
		prefix = prefix.Replace("reset","zero");
		if(setMethod is MethodInt){
			int current = (int)((MethodReturn)getMethod)();
			int adjust = (int)value;
			if(prefix == "add"){current += adjust;}
			if(prefix == "multiply"){current *= adjust;}
			if(prefix == "divide"){current /= adjust;}
			if(prefix == "subtract"){current -= adjust;}
			if(prefix == "max"){current = Mathf.Max(current,adjust);}
			if(prefix == "min"){current = Mathf.Min(current,adjust);}
			if(prefix == "average"){current = ((current + adjust)) / 2;}
			if(prefix == "zero"){current = 0;}
			((MethodInt)setMethod)(current);
		}
		if(setMethod is MethodFloat){
			float original = (float)((MethodReturn)getMethod)();
			float adjust = (float)value;
			float current = original;
			if(prefix == "add"){current += adjust;}
			if(prefix == "multiply"){current *= adjust;}
			if(prefix == "divide"){current /= adjust;}
			if(prefix == "subtract"){current -= adjust;}
			if(prefix == "max"){current = Mathf.Max(current,adjust);}
			if(prefix == "min"){current = Mathf.Min(current,adjust);}
			if(prefix == "average"){current = ((current + adjust)) / 2;}
			if(prefix == "zero"){current = 0;}
			((MethodFloat)setMethod)(current);
		}
		if(setMethod is MethodVector3){
			Vector3 original = (Vector3)((MethodReturn)getMethod)();
			Vector3 adjust = (Vector3)value;
			Vector3 current = original;
			if(prefix == "add"){current += adjust;}
			if(prefix == "multiply"){current = Vector3.Scale(current,adjust);}
			//if(prefix == "divide"){current = current.Divide(adjust);}
			if(prefix == "subtract"){current -= adjust;}
			if(prefix == "max"){current = Vector3.Max(current,adjust);}
			if(prefix == "min"){current = Vector3.Min(current,adjust);}
			if(prefix == "average"){current = ((current + adjust)) / 2;}
			if(prefix == "zero"){current = Vector3.zero;}
			if(prefix == "x"){current = Vector3.zero;}
			if(prefix == "y"){current = Vector3.zero;}
			if(prefix == "z"){current = Vector3.zero;}
			if(suffix.ContainsAny("x","y","z")){
				if(suffix == "x"){original.x = current.x;}
				if(suffix == "y"){original.y = current.y;}
				if(suffix == "z"){original.z = current.z;}
				current = original;
			}
			((MethodVector3)setMethod)(current);
		}
	}
	private static object GetSpecialCallback(GameObject target,string name,string callbackName){
		string prefix = Events.GetSpecialPrefix(target,name);
		string suffix = Events.GetSpecialSuffix(target,name);
		if(!prefix.IsEmpty() || !suffix.IsEmpty()){
			name = callbackName + name.TrimLeft(prefix).TrimRight(suffix);
			if(Events.HasEvent(target,name)){
				return Events.objectEvents[target][name].First();
			}
		}
		return null;
	}
	private static string GetSpecialPrefix(GameObject target,string name){
		string[] startKeys = new string[]{"add","multiply","divide","subtract","max","min","average","zero","reset"};
		foreach(string key in startKeys){
			if(name.StartsWith(key,true)){
				return key;
			}
		}
		return "";
	}
	private static string GetSpecialSuffix(GameObject target,string name){
		string[] endKeys = new string[]{"x","y","z"};
		foreach(string key in endKeys){
			if(name.EndsWith(key,true)){
				//return key;
			}
		}
		return "";
	}
	public static object Query(string name,object result=null,params object[] values){
		name = name.ToLower();
		if(Events.events.ContainsKey(name)){
			foreach(object callback in Events.events[name]){
				return Events.HandleGet(callback,values);
			}
		}
		return result;
	}
	public static object Query(GameObject target,string name,object[] values,object result=null){
		name = name.ToLower();
		if(Events.objectEvents.ContainsKey(target)){
			if(Events.objectEvents[target].ContainsKey(name)){
				foreach(object callback in Events.objectEvents[target][name]){
					return Events.HandleGet(callback,values);
				}
			}
		}
		return result;
	}
	public static object QueryChildren(GameObject target,string name,object[] values,object result=null,bool self=false){
		if(result != null){return result;}
		if(self){Events.Query(target,name,values,result);}
		Transform[] children = target.GetComponentsInChildren<Transform>();
		foreach(Transform transform in children){
			if(transform.gameObject == target){continue;}
			result = Events.QueryChildren(transform.gameObject,name,values,result,true);
			if(result != null){return result;}
		}
		return result;
	}
	public static object QueryParents(GameObject target,string name,object[] values,object result=null,bool self=false){
		if(result != null){return result;}
		if(self){Events.Query(target,name,values,result);}
		Transform parent = target.transform.parent;
		while(parent != null){
			result = Events.QueryParents(parent.gameObject,name,values,result,true);
			parent = parent.parent;
			if(result != null){return result;}
		}
		return result;
	}
	public static object QueryFamily(GameObject target,string name,object[] values,object result=null,bool self=false){
		if(self){result = Events.Query(target,name,values,result);}
		result = Events.QueryChildren(target,name,values,result,false);
		result = Events.QueryParents(target,name,values,result,false);
		return result;
	}
	public static void Call(string name,params object[] values){
		name = name.ToLower();
		if(Events.events.ContainsKey(name)){
			foreach(object callback in Events.events[name]){
				Events.Handle(callback,values);
			}
			return;
		}
		//Events.CheckSpecial(name,values);
	}
	public static void Call(GameObject target,string name,object[] values){
		name = name.ToLower();
		if(Events.objectEvents.ContainsKey(target)){
			if(Events.objectEvents[target].ContainsKey(name)){
				foreach(object callback in Events.objectEvents[target][name]){
					Events.Handle(callback,values);
				}
				return;
			}
			Events.CheckSpecial(target,name,values);
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
	private static bool HasEvent(GameObject target,string name){
		if(Events.objectEvents.ContainsKey(target)){
				return Events.objectEvents[target].ContainsKey(name);
		}
		return false;
	}
	private static bool HasEvent(string name){
		return Events.events.ContainsKey(name);
	}
	private static object GetEvent(GameObject target,string name){
		if(Events.objectEvents.ContainsKey(target)){
				if(Events.objectEvents[target].ContainsKey(name)){
					return Events.objectEvents[target][name];
				}
		}
		return null;
	}
	private static object GetEvent(string name){
		if(Events.events.ContainsKey(name)){
			return Events.events[name];
		}
		return null;
	}
}
public static class GameObjectEvents{
	public static object Query(this GameObject current,string name,params object[] values){
		return Events.Query(current,name,values);
	}
	public static object QueryChildren(this GameObject current,string name,bool self=true,params object[] values){
		return Events.QueryChildren(current,name,values,null,self);
	}
	public static object QueryParents(this GameObject current,string name,bool self=true,params object[] values){
		return Events.QueryParents(current,name,values,null,self);
	}
	public static object QueryFamily(this GameObject current,string name,bool self=true,params object[] values){
		return Events.QueryFamily(current,name,values,null,self);
	}
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
