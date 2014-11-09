using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor;
#endif
public static class Events{
	public static Dictionary<GameObject,Dictionary<string,List<object>>> objectEvents = new Dictionary<GameObject,Dictionary<string,List<object>>>();
	public static Dictionary<string,List<object>> events = new Dictionary<string,List<object>>();
	static Events(){Events.Clear();}
	public static void Clear(){
		Events.objectEvents.Clear();
		Events.events.Clear();
	}
	public static void Empty(){}
	public static void Register(string name){Events.Add(name,()=>{});}
	public static void Register(string name,params GameObject[] targets){
		foreach(GameObject target in targets){
			Events.AddScope(name,(Method)Events.Empty,target);
		}
	}
	public static void Add(string name,Method method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void Add(string name,MethodObject method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void Add(string name,MethodFull method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void Add(string name,MethodString method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void Add(string name,MethodInt method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void Add(string name,MethodFloat method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void Add(string name,MethodBool method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void Add(string name,MethodVector2 method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void Add(string name,MethodVector3 method,params GameObject[] targets){Events.Add(name,(object)method,targets);}
	public static void AddScope(string name,object method,GameObject target){
		Events.Clean(name,target,method);
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
	public static void Add(string name,object method,params GameObject[] targets){
		//name = name.ToLower();
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
				Events.AddScope(name,method,target);
			}
			foreach(GameObject target in targets){
				if(target.IsNull()){continue;}
				Events.AddScope(name,method,target);
			}
		}
	}
	public static void Clean(string ignoreName="",GameObject target=null,object targetMethod=null){
		#if UNITY_EDITOR 
		if(!EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying){
			foreach(var current in Events.objectEvents.Copy()){
				GameObject gameObject = current.Key;
				if(gameObject.IsNull()){
					Events.objectEvents.Remove(gameObject);
					continue;
				}
				foreach(var item in current.Value.Copy()){
					string eventName = item.Key;
					foreach(object method in item.Value.Copy()){
						//if(method.Equals((object)(Method)Events.Empty)){continue;}
						bool duplicate = eventName != ignoreName && target == gameObject && method.Equals(targetMethod);
						bool invalid = method == null || ((Delegate)method).Target.IsNull();
						if(duplicate || invalid){
							var copy = item.Value.Copy();
							copy.Remove(method);
							//string messageType = method == null ? "empty method" : "duplicate method";
							//Debug.Log("Events : Removing " + messageType  + " from -- " + gameObject.name + "/" + eventName);
							Events.objectEvents[gameObject][eventName] = copy;
						}
					}
					if(Events.objectEvents[gameObject][eventName].Count < 1){
						//Debug.Log("Events : Removing empty method list -- " + gameObject.name + "/" + eventName);
						Events.objectEvents[gameObject].Remove(eventName);
					}
				}
			}
		}
		#endif
	}
	public static List<string> GetEvents(GameObject target=null){
		Events.Clean();
		if(target.IsNull()){
			return Events.events.Keys.ToList();
		}
		if(Events.objectEvents.ContainsKey(target)){
			return Events.objectEvents[target].Keys.ToList();
		}
		return null;
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
	public static void Call(string name,params object[] values){
		if(Events.events.ContainsKey(name)){
			foreach(object callback in Events.events[name]){
				Events.Handle(callback,values);
			}
			return;
		}
	}
	public static void Call(GameObject target,string name,object[] values){
		if(!Events.ValidTarget(target,name)){return;}
		List<object> invalid = new List<object>();
		if(Events.objectEvents.ContainsKey(target)){
			if(Events.objectEvents[target].ContainsKey(name)){
				foreach(object callback in Events.objectEvents[target][name]){
					if(callback == null || ((Delegate)callback).Target.IsNull()){
						invalid.Add(callback);
						continue;
					}
					Events.Handle(callback,values);
				}
				foreach(object entry in invalid){
					Events.objectEvents[target][name].Remove(entry);
				}
				return;
			}
		}
	}
	public static void CallChildren(GameObject target,string name,object[] values,bool self=false){
		if(!Events.ValidTarget(target,name)){return;}
		if(self){Events.Call(target,name,values);}
		Transform[] children = target.GetComponentsInChildren<Transform>();
		foreach(Transform transform in children){
			if(transform.gameObject == target){continue;}
			Events.CallChildren(transform.gameObject,name,values,true);
		}
	}
	public static void CallParents(GameObject target,string name,object[] values,bool self=false){
		if(!Events.ValidTarget(target,name)){return;}
		if(self){Events.Call(target,name,values);}
		Transform parent = target.transform.parent;
		while(parent != null){
			Events.CallParents(parent.gameObject,name,values,true);
			parent = parent.parent;
		}
	}
	public static void CallFamily(GameObject target,string name,object[] values,bool self=false){
		if(!Events.ValidTarget(target,name)){return;}
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
	private static bool ValidTarget(GameObject target,string name){
		if(target.IsNull()){
			Debug.LogWarning("Events : Call attempted on null gameObject -- " + name);
			return false;
		}
		return true;
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
	public static void Register(this GameObject current,string name,params object[] values){
		Events.Register(name,current);
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
