using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR 
using UnityEditor;
#endif
namespace Zios{
    public static class Events{
		public static bool debug;
	    public static GameObject all = new GameObject("All Events");
	    public static GameObject global = new GameObject("Global Events");
	    public static List<string> warned = new List<string>();
	    public static Dictionary<UnityObject,Dictionary<string,List<object>>> listeners = new Dictionary<UnityObject,Dictionary<string,List<object>>>();
	    public static Dictionary<UnityObject,List<string>> callers = new Dictionary<UnityObject,List<string>>();
	    public static Dictionary<UnityObject,float> lastRegister = new Dictionary<UnityObject,float>();
	    static Events(){
		    Events.all.hideFlags = HideFlags.HideAndDontSave;
		    Events.global.hideFlags = HideFlags.HideAndDontSave;
	    }
	    public static void Empty(){}
	    public static void Register(string name){Events.Register(name,Events.global);}
	    public static void Register(string name,params UnityObject[] targets){
		    targets = targets.Add(Events.all);
		    foreach(UnityObject target in targets){
			    Events.lastRegister.AddNew(target);
			    if(Time.realtimeSinceStartup > Events.lastRegister[target]){
				    Events.callers[target] = new List<string>();
				    Events.lastRegister[target] = Time.realtimeSinceStartup + 1;
			    }
			    if(!Events.callers.ContainsKey(target)){Events.callers[target] = new List<string>();}
			    if(!Events.callers[target].Contains(name)){Events.callers[target].Add(name);}
		    }
	    }
	    public static void Add(string name,Method method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void Add(string name,MethodObject method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void Add(string name,MethodFull method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void Add(string name,MethodString method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void Add(string name,MethodInt method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void Add(string name,MethodFloat method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void Add(string name,MethodBool method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void Add(string name,MethodVector2 method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void Add(string name,MethodVector3 method,params UnityObject[] targets){Events.Add(name,(object)method,targets);}
	    public static void AddScope(string name,object method,UnityObject target){
		    //Utility.EditorCall(()=>Events.Clean(name,target,method));
		    if(!Events.listeners.ContainsKey(target)){
			    Events.listeners[target] = new Dictionary<string,List<object>>();
		    }
		    if(!Events.listeners[target].ContainsKey(name)){
			    Events.listeners[target][name] = new List<object>();
		    }
		    if(!Events.listeners[target][name].Contains(method)){
			    Events.listeners[target][name].Add(method);
		    }
	    }
	    public static void Add(string name,object method,params UnityObject[] targets){
		    Events.AddScope(name,method,Events.all);
		    if(targets.Contains(Events.global)){
				if(Events.debug){Debug.Log("[EventManager] : Adding global event -- " + name + " -- " + method);}
			    Events.AddScope(name,method,Events.global);
			    return;
		    }
		    foreach(UnityObject target in targets){
			    if(target.IsNull()){continue;}
				if(Events.debug){Debug.Log("[EventManager] : Adding target event -- " + target + " -- " + name + " -- " + method);}
			    Events.AddScope(name,method,target);
		    }
		    object methodTarget = ((Delegate)method).Target;
		    if(methodTarget != null){
			    if(methodTarget is MonoBehaviour){ 
				    UnityObject target = ((MonoBehaviour)methodTarget).gameObject;
					if(Events.debug){Debug.Log("[EventManager] : Method is from a MonoBehaviour.  Auto-adding event to gameObject -- " + target);}
				    Events.AddScope(name,method,target);
			    }
		    }
	    }
	    public static void Clean(string ignoreName="",UnityObject target=null,object targetMethod=null){
		    foreach(var current in Events.listeners.Copy()){
			    UnityObject unityObject = current.Key;
			    if(unityObject.IsNull()){
				    Events.listeners.Remove(unityObject);
				    continue;
			    }
			    foreach(var item in current.Value.Copy()){
				    string eventName = item.Key;
				    foreach(object method in item.Value.Copy()){
					    //if(method.Equals((object)(Method)Events.Empty)){continue;}
					    bool duplicate = eventName != ignoreName && target == unityObject && method.Equals(targetMethod);
					    bool invalid = method == null || ((Delegate)method).Target.IsNull();
					    if(duplicate || invalid){
						    var copy = item.Value.Copy();
						    copy.Remove(method);
						    //string messageType = method == null ? "empty method" : "duplicate method";
						    //Debug.Log("[Events] Removing " + messageType  + " from -- " + unityObject.name + "/" + eventName);
						    Events.listeners[unityObject][eventName] = copy;
					    }
				    }
				    if(Events.listeners[unityObject][eventName].Count < 1){
					    //Debug.Log("[Events] Removing empty method list -- " + unityObject.name + "/" + eventName);
					    Events.listeners[unityObject].Remove(eventName);
				    }
			    }
		    }
		    foreach(var current in Events.callers.Copy()){
			    UnityObject unityObject = current.Key;
			    if(unityObject.IsNull()){
				    Events.callers.Remove(unityObject);
			    }
		    }
	    }
	    public static bool HasEvents(string type,UnityObject target=null){
		    if(target.IsNull()){target = Events.global;}
		    if(type.Contains("Listen",true)){
			    return Events.listeners.ContainsKey(target);
		    }
		    return Events.callers.ContainsKey(target);
	    }
	    public static bool ContainsEvent(string type,string name,UnityObject target=null){
		    if(target.IsNull()){target = Events.global;}
		    if(type.Contains("Listen",true)){
			    return Events.listeners.ContainsKey(target) && Events.listeners[target].ContainsKey(name);
		    }
		    return Events.callers.ContainsKey(target) && Events.callers[target].Contains(name);
	    }
	    public static List<string> GetEvents(string type,UnityObject target=null){
		    Utility.EditorCall(Events.Clean);
		    if(target.IsNull()){target = Events.global;}
		    if(type.Contains("Listen",true)){
			    if(Events.listeners.ContainsKey(target)){
				    return Events.listeners[target].Keys.ToList();
			    }
		    }
		    if(Events.callers.ContainsKey(target)){
			    return Events.callers[target];
		    }
		    return new List<string>();
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
		    Events.Call(Events.global,name,values);
	    }
	    public static void Call(UnityObject target,string name,object[] values){
		    if(!Events.ValidTarget(name,target)){return;}
		    List<object> invalid = new List<object>();
			if(Events.debug){Debug.Log("[EventManager] : Calling event -- " + name + " on " + target);}
		    if(Events.listeners.ContainsKey(target)){
				if(Events.debug){Debug.Log("[EventManager] : Event target found -- " + target);}
			    if(Events.listeners[target].ContainsKey(name)){
					if(Events.debug){Debug.Log("[EventManager] : Event target name found -- " + name);}
				    foreach(object callback in Events.listeners[target][name]){
					    if(callback == null){
							if(Events.debug){Debug.Log("[EventManager] : Invalid callback -- " + callback);}
						    invalid.Add(callback);
						    continue;
					    }
						if(Events.debug){Debug.Log("[EventManager] : Calling event -- " + callback);}
					    Events.Handle(callback,values);
				    }
				    foreach(object entry in invalid){
					    Events.listeners[target][name].Remove(entry);
				    }
				    return;
			    }
		    }
	    }
	    public static void CallChildren(UnityObject target,string name,object[] values,bool self=false){
		    if(!Events.ValidTarget(name,target)){return;}
		    if(self){Events.Call(target,name,values);}
			if(target is GameObject){
				var gameObject = (GameObject)target;
				Transform[] children = gameObject.GetComponentsInChildren<Transform>();
				foreach(Transform transform in children){
					if(transform.gameObject == gameObject){continue;}
					Events.CallChildren(transform.gameObject,name,values,true);
				}
			}
	    }
	    public static void CallParents(UnityObject target,string name,object[] values,bool self=false){
		    if(!Events.ValidTarget(name,target)){return;}
		    if(self){Events.Call(target,name,values);}
			if(target is GameObject){
				var gameObject = (GameObject)target;
				Transform parent = gameObject.transform.parent;
				while(parent != null){
					Events.CallParents(parent.gameObject,name,values,true);
					parent = parent.parent;
				}
			}
	    }
	    public static void CallFamily(UnityObject target,string name,object[] values,bool self=false){
		    if(!Events.ValidTarget(name,target)){return;}
		    if(self){Events.Call(target,name,values);}
		    Events.CallChildren(target,name,values);
		    Events.CallParents(target,name,values);
	    }
	    public static bool ValidTarget(string name,UnityObject target){
		    if(target.IsNull()){
			    if(!Events.warned.Contains(name)){
				    Debug.LogWarning("[Events] Call attempted on null unityObject -- " + name,target);
				    Events.warned.Add(name);
			    }
			    return false;
		    }
		    return true;
	    }
	    public static bool HasEvent(string name,UnityObject target=null){
		    if(target.IsNull()){target = Events.global;}
		    return Events.listeners.ContainsKey(target) && Events.listeners[target].ContainsKey(name);
	    }
	    public static object GetEvent(string name,UnityObject target=null){
		    if(target.IsNull()){target = Events.global;}
		    if(Events.listeners.ContainsKey(target)){
			    if(Events.listeners[target].ContainsKey(name)){
				    return Events.listeners[target][name];
			    }
		    }
		    return null;
	    }
    }
    public static class UnityObjectListeners{
	    public static void RegisterEvent(this UnityObject current,string name,params object[] values){
		    Events.Register(name,current);
	    }
	    public static void AddEvent(this UnityObject current,string name,object method){
		    Events.Add(name,method,current);
	    }
	    public static void CallEvent(this UnityObject current,string name,params object[] values){
		    Events.Call(current,name,values);
	    }
	    public static void CallEventChildren(this UnityObject current,string name,bool self=true,params object[] values){
		    Events.CallChildren(current,name,values,self);
	    }
	    public static void CallEventParents(this UnityObject current,string name,bool self=true,params object[] values){
		    Events.CallParents(current,name,values,self);
	    }
	    public static void CallEventFamily(this UnityObject current,string name,bool self=true,params object[] values){
		    Events.CallFamily(current,name,values,self);
	    }
    }
}