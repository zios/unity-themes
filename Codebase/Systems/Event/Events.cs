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
	#if UNITY_EDITOR 
	[InitializeOnLoad]
	public static class EventsBooter{
		static EventsBooter(){
			Events.Add("On Hierarchy Changed",Events.Build).SetPermanent(true);
			if(!Application.isPlaying){
				Utility.EditorDelayCall(Events.Build);
				Utility.EditorDelayCall(Events.Setup);
			}
		}
	}
	#endif
	[Flags]
    public enum EventDebug : int{
	    Add        = 0x001,
	    AddDeep    = 0x002,
	    Remove     = 0x004,
	    Call       = 0x008,
	    CallEmpty  = 0x010,
	    CallDeep   = 0x020,
	    CallTimer  = 0x040,
		CallUpdate = 0x080,
		Pause      = 0x100,
    }
    public enum EventDebugScope : int{
		Global     = 0x001,
		Scoped     = 0x002,
    }
	[Serializable]
	public class EventListener{
		public object target;
		public object method;
		public string name;
		public int occurrences;
		public bool paused;
		public bool permanent;
		private bool warned;
	    public bool IsValid(){
			if(this.target.IsNull() && !this.warned && Events.debug.Has("Call")){
				Debug.LogWarning("[Events] Call attempted -- null unityObject -- " + this.name,(UnityObject)this.target);
				this.warned = true;
			}
		    return !this.target.IsNull();
	    }
		public void SetPaused(bool state){this.paused = state;}
		public void SetPermanent(bool state){this.permanent = state;}
	    public void Call(object[] values){
			if(!this.IsValid()){return;}
			if(this.occurrences > 0){this.occurrences -= 1;}
			if(values.Length < 1 || this.method is Method){
				((Method)this.method)();
				return;
			}
		    object value = values[0];
		    if(this.method is MethodFull){((MethodFull)this.method)(values);}
		    else if(value is object && this.method is MethodObject){((MethodObject)this.method)((object)value);}
		    else if(value is int && this.method is MethodInt){((MethodInt)this.method)((int)value);}
		    else if(value is float && this.method is MethodFloat){((MethodFloat)this.method)((float)value);}
		    else if(value is string && this.method is MethodString){((MethodString)this.method)((string)value);}
		    else if(value is bool && this.method is MethodBool){((MethodBool)this.method)((bool)value);}
		    else if(value is Vector2 && this.method is MethodVector2){((MethodVector2)this.method)((Vector2)value);}
		    else if(value is Vector3 && this.method is MethodVector3){((MethodVector3)this.method)((Vector3)value);}
	    }
	}
    public class Events : EventDetector{
		public static bool disabled;
		[EnumMask] public static EventDebugScope debugScope = (EventDebugScope)(-1);
		[EnumMask] public static EventDebug debug;
		public static Events instance;
	    public static object all = "All";
	    public static object global = "Global";
		public static EventListener empty = new EventListener();
		public static Dictionary<object,Dictionary<string,Dictionary<object,EventListener>>> cache = new Dictionary<object,Dictionary<string,Dictionary<object,EventListener>>>();
	    public static List<EventListener> listeners = new List<EventListener>();
	    public static Dictionary<object,List<string>> callers = new Dictionary<object,List<string>>();
		public static string lastEventName;
		public static void StaticValidate(){
			PlayerPrefs.SetInt("Events-Debug",Events.debug.ToInt());
			PlayerPrefs.SetInt("Events-DebugScope",Events.debugScope.ToInt());
		}
		public static void Build(){
			if(Events.instance.IsNull()){
				var eventsPath = Locate.GetScenePath("@Main/Events");
				if(!eventsPath.HasComponent<Events>()){
					Debug.Log("[EventManager] : Auto-creating Events Manager GameObject.");
					Events.instance = eventsPath.AddComponent<Events>();
				}
				Events.instance = eventsPath.GetComponent<Events>();
			}
		}
		public static void Setup(){
			Events.debug = (EventDebug)PlayerPrefs.GetInt("Events-Debug");
			Events.debugScope = (EventDebugScope)PlayerPrefs.GetInt("Events-DebugScope");
			Events.callers.Clear();
			Events.cache.Clear();
			Events.listeners.RemoveAll(x=>!x.permanent);
			foreach(var listener in Events.listeners){
				var scope = Events.cache.AddNew(listener.target).AddNew(listener.name);
				scope[listener.method] = listener;
			}
			Events.Call("On Events Reset");
		}
	    public override void Awake(){
			//Events.Add("On Hierarchy Changed",this.Awake);
			Events.instance = this;
			Events.Setup();
			base.Awake();
		}
		public override void OnDestroy(){
			Events.instance = null;
			base.OnDestroy();
		}
		public static object Validate(object target=null){
			if(target.IsNull()){target = Events.global;}
			return target;
		}
		public static object[] ValidateAll(params object[] targets){
			if(targets.Length < 1){targets = new object[1]{Events.global};}
			return targets;
		}
	    public static void Empty(){}
	    public static void Register(string name){Events.Register(name,Events.Validate());}
	    public static void Register(string name,params object[] targets){
			if(Events.disabled){return;}
		    foreach(object target in targets){
				if(target.IsNull()){continue;}
				Events.callers.AddNew(target);
				Events.callers[target].AddNew(name);
		    }
	    }
	    public static EventListener Add(string name,Method method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener Add(string name,MethodObject method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener Add(string name,MethodFull method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener Add(string name,MethodString method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener Add(string name,MethodInt method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener Add(string name,MethodFloat method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener Add(string name,MethodBool method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener Add(string name,MethodVector2 method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener Add(string name,MethodVector3 method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
	    public static EventListener AddLimited(string name,Method method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener AddLimited(string name,MethodObject method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener AddLimited(string name,MethodFull method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener AddLimited(string name,MethodString method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener AddLimited(string name,MethodInt method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener AddLimited(string name,MethodFloat method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener AddLimited(string name,MethodBool method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener AddLimited(string name,MethodVector2 method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener AddLimited(string name,MethodVector3 method,int amount,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
	    public static EventListener Add(string name,object method,int amount,params object[] targets){
			if(Events.disabled){
				if(Events.debug.Has("AddDeep")){Debug.LogWarning("[EventManager] : Add attempted while Events disabled. " + name);}
				return null;
			}
			targets = Events.ValidateAll(targets);
			var listener = Events.empty;
			foreach(object target in targets){
			    if(target.IsNull()){continue;}
				if(!Events.cache.AddNew(target).AddNew(name).ContainsKey(method)){
					listener = new EventListener();
					if(Events.debug.Has("Add")){
						var info = (Delegate)method;
						Debug.Log("[EventManager] : Adding event -- " + Events.GetMethodName(info) + " -- " + name,target as UnityObject);
					}
					Events.listeners.Add(listener);
					Events.cache.AddNew(Events.all).AddNew(name);
				}
				else{
					listener = Events.cache[target][name].AddNew(method);
				}
				listener.name = name;
				listener.method = method;
				listener.target = target;
				listener.occurrences = amount;
				Events.cache[target][name][method] = listener;
				Events.cache[Events.all][name][method] = listener;
		    }
			return listener;
	    }
	    public static void Remove(string name,Method method,params object[] targets){Events.Remove(name,(object)method,targets);}
	    public static void Remove(string name,MethodObject method,params object[] targets){Events.Remove(name,(object)method,targets);}
	    public static void Remove(string name,MethodFull method,params object[] targets){Events.Remove(name,(object)method,targets);}
	    public static void Remove(string name,MethodString method,params object[] targets){Events.Remove(name,(object)method,targets);}
	    public static void Remove(string name,MethodInt method,params object[] targets){Events.Remove(name,(object)method,targets);}
	    public static void Remove(string name,MethodFloat method,params object[] targets){Events.Remove(name,(object)method,targets);}
	    public static void Remove(string name,MethodBool method,params object[] targets){Events.Remove(name,(object)method,targets);}
	    public static void Remove(string name,MethodVector2 method,params object[] targets){Events.Remove(name,(object)method,targets);}
	    public static void Remove(string name,MethodVector3 method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,object method,params object[] targets){
			if(Events.disabled){return;}
			targets = Events.ValidateAll(targets);
			foreach(var target in targets){
				if(Events.cache.ContainsKey(target) && Events.cache[target].ContainsKey(name)){
					Events.cache[target][name].Remove(method);
					Events.cache[Events.all][name].Remove(method);
				}
				Events.listeners.RemoveAll(x=>x.method==method && x.target==target && x.name==name);
			}
		}
	    public static void SetPause(string type,string name,object method,object target){
			target = Events.Validate(target);
			if(Events.debug.Has("Pause")){
				string message = "[EventManager] : " + type + " event -- " + Events.GetTargetName(target) + " -- " + name;
				Debug.Log(message,target as UnityObject);
			}
			var events = Events.cache.AddNew(target).AddNew(name);
			foreach(var item in events){
				item.Value.paused = type == "Pausing";
			}
		}
	    public static void Pause(string name,object method=null,object target=null){Events.SetPause("Pausing",name,method,target);}
	    public static void Resume(string name,object method=null,object target=null){Events.SetPause("Resuming",name,method,target);}
	    public static void Call(string name,params object[] values){
			if(Events.disabled){return;}
			Events.lastEventName = name;
		    Events.Call(Events.Validate(),name,values);
	    }
	    public static void Call(object target,string name,object[] values){
			if(Events.disabled){return;}
			Events.lastEventName = name;
			var events = Events.cache.AddNew(target).AddNew(name);
			if(events.Count == 0 && !Events.debug.Has("CallEmpty")){return;}
			float callTime = Time.realtimeSinceStartup;
			bool debugEvent = Events.CanDebug(target,name,events.Count);
			foreach(var item in events.Copy()){
				var listener = item.Value;
				if(listener.paused){continue;}
				if(debugEvent && Events.debug.Has("CallDeep")){
					string message = "[EventManager] : Event calling -- " + Events.GetMethodName(listener.method);
					Debug.Log(message,target as UnityObject);
				}
				listener.Call(values);
				if(listener.occurrences == 0){
					Events.listeners.Remove(listener);
					Events.cache[target][name].Remove(listener.method);
				}
			}
			if(debugEvent && Events.debug.Has("CallTimer")){
				float elapsed = Time.realtimeSinceStartup - callTime;
				string message = "[EventManager] : Calling complete [" + events.Count + "] -- " + name + " -- " + elapsed + " seconds.";
				Debug.Log(message,target as UnityObject);
			}
	    }
	    public static void CallChildren(object target,string name,object[] values,bool self=false){
			if(Events.disabled){return;}
		    if(self){Events.Call(target,name,values);}
			if(target is GameObject){
				var gameObject = (GameObject)target;
				Transform[] children = Locate.GetObjectComponents<Transform>(gameObject);
				foreach(Transform transform in children){
					if(transform.gameObject == gameObject){continue;}
					Events.CallChildren(transform.gameObject,name,values,true);
				}
			}
	    }
	    public static void CallParents(object target,string name,object[] values,bool self=false){
			if(Events.disabled){return;}
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
	    public static void CallFamily(object target,string name,object[] values,bool self=false){
			if(Events.disabled){return;}
		    if(self){Events.Call(target,name,values);}
		    Events.CallChildren(target,name,values);
		    Events.CallParents(target,name,values);
	    }
		//========================
		// Editor
		//========================
		public static string GetTargetName(object target){
			if(target.IsNull()){return "Null";}
			string targetName = target.ToString();
			if(target is GameObject){targetName = ((GameObject)target).GetPath();}
			if(target is Component){targetName = ((Component)target).GetPath();}
			if(target is Attribute){targetName = ((Attribute)target).info.parent.GetPath();}
			if(target is AttributeData){targetName = ((AttributeData)target).attribute.parent.GetPath();}
			if(targetName.IsEmpty()){targetName = target.GetType().Name;}
			targetName = targetName.Trim("/");
			if(targetName.Contains("__")){targetName = "Anonymous";}
			return targetName;
		}
		public static string GetMethodName(object method){
			string name = "Unknown";
			if(method is Delegate){
				var info = (Delegate)method;
				string targetName = info.Method.DeclaringType.Name;
				string methodName = info.Method.Name;
				if(!info.Target.IsNull()){
					string path = "";
					if(info.Target is Component){path = info.Target.As<Component>().GetPath(false);}
					if(info.Target is GameObject){path = info.Target.As<GameObject>().GetPath();}
					if(!path.IsEmpty()){path = path.Trim("/") + " -- ";}
					targetName = path + info.Target.GetAlias().Replace(" ","");
					if(targetName.Contains("__")){targetName = info.Target.GetType().ReflectedType.Name;}
				}
				if(targetName.Contains("__")){targetName = "Anonymous";}
				if(methodName.Contains("__")){methodName = "Anonymous";}
				name = targetName + "." + methodName + "()";
			}
			return name;
		}
		public static bool CanDebug(object target,string name,int count){
			bool allowed = true;
			var debug = Events.debug;
			var scope = Events.debugScope;
			allowed = target.Equals(Events.global) ? scope.Has("Global") : scope.Has("Scoped");
			if(allowed && name.ContainsAny("On Update","On Editor Update","On GUI")){
				allowed = debug.Has("CallUpdate");
			}
			if(allowed && debug.HasAny("Call","CallUpdate","CallDeep","CallEmpty")){
				string message = "[EventManager] : Calling " + count + " events -- " + Events.GetTargetName(target) + " -- " + name;
				if(allowed){Debug.Log(message,target as UnityObject);}
			}
			return allowed;
		}
	    public static void Clean(string ignoreName="",object target=null,object targetMethod=null){
		    foreach(var eventListener in Events.listeners){
				string eventName = eventListener.name;
				object eventTarget = eventListener.target;
				object eventMethod = eventListener.method;
				bool duplicate = eventName != ignoreName && eventTarget == target && eventMethod.Equals(targetMethod);
				bool invalid = eventTarget.IsNull() || eventMethod.IsNull() || ((Delegate)eventMethod).Target.IsNull();
				if(duplicate || invalid){
				    Utility.EditorDelayCall(()=>Events.listeners.Remove(eventListener));
					if(Events.debug.Has("Remove")){
						string messageType = eventMethod.IsNull() ? "empty method" : "duplicate method";
						string message = "[Events] Removing " + messageType  + " from -- " + eventTarget + "/" + eventName;
						Debug.Log(message,target as UnityObject);
					}
				}
		    }
		    foreach(var current in Events.callers){
			    object scope = current.Key;
			    if(scope.IsNull()){
				    Utility.EditorDelayCall(()=>Events.callers.Remove(scope));
			    }
		    }
	    }
	    public static bool HasEvents(string name,object target=null){
			target = Events.Validate(target);
			return Events.cache.AddNew(target).AddNew(name).Count < 1;
	    }
	    public static List<string> GetEventNames(string type,object target=null){
		    Utility.EditorCall(Events.Clean);
			target = Events.Validate(target);
		    if(type.Contains("Listen",true)){
			    return Events.listeners.ToList().FindAll(x=>x.target==target).Select(x=>x.name).ToList();
		    }
		    if(Events.callers.ContainsKey(target)){
			    return Events.callers[target];
		    }
		    return new List<string>();
	    }
    }
    public static class ObjectEventExtensions{
	    public static void RegisterEvent(this object current,string name,params object[] values){
		    Events.Register(name,current);
	    }
	    public static EventListener AddEvent(this object current,string name,object method,int amount=-1){
		    return Events.Add(name,method,amount,current);
	    }
	    public static void RemoveEvent(this object current,string name,object method){
		    Events.Remove(name,method,current);
	    }
	    public static void CallEvent(this object current,string name,params object[] values){
		    Events.Call(current,name,values);
	    }
	    public static void CallEventChildren(this object current,string name,bool self=true,params object[] values){
		    Events.CallChildren(current,name,values,self);
	    }
	    public static void CallEventParents(this object current,string name,bool self=true,params object[] values){
		    Events.CallParents(current,name,values,self);
	    }
	    public static void CallEventFamily(this object current,string name,bool self=true,params object[] values){
		    Events.CallFamily(current,name,values,self);
	    }
    }
}