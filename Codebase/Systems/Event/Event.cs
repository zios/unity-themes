#pragma warning disable 0618
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Events{
	using Containers;
	[InitializeOnLoad]
	public static class EventsHook{
		public static Hook<Event> hook;
		static EventsHook(){
			if(Application.isPlaying){return;}
			#if !UNITY_THEMES
			EventsHook.hook = new Hook<Event>(EventsHook.Reset,EventsHook.Create);
			#endif
		}
		public static void Reset(){
			EventsHook.hook.Reset();
			if(Event.instance){
				Event.Add("On Level Was Loaded",Event.instance.Awake);
			}
		}
		public static void Create(){
			EventsHook.hook.Create();
			if(Event.instance.IsNull()){
				Event.Cleanup();
			}
		}
	}
	//=======================
	// Enumerations
	//=======================
	[Flags]
	public enum EventDisabled : int{
		Add        = 0x001,
		Call       = 0x002,
	}
	[Flags]
	public enum EventDebug : int{
		Add            = 0x001,
		Remove         = 0x002,
		Call           = 0x004,
		CallEmpty      = 0x008,
		CallDeep       = 0x010,
		CallTimer      = 0x020,
		CallTimerZero  = 0x040,
		CallUpdate     = 0x080,
		Pause          = 0x100,
		History        = 0x200,
	}
	public enum EventDebugScope : int{
		Global     = 0x001,
		Scoped     = 0x002,
	}
	//=======================
	// Delegates
	//=======================
	public delegate void Method();
	public delegate void MethodObject(object value);
	public delegate void MethodInt(int value);
	public delegate void MethodFloat(float value);
	public delegate void MethodString(string value);
	public delegate void MethodBool(bool value);
	public delegate void MethodVector2(Vector2 value);
	public delegate void MethodVector3(Vector3 value);
	public delegate void MethodFull(object[] values);
	public delegate void MethodStep(object collection,int value);
	public delegate object MethodReturn();
	public delegate object MethodObjectReturn(object value);
	public delegate object MethodIntReturn(int value);
	public delegate object MethodFloatReturn(float value);
	public delegate object MethodStringReturn(string value);
	public delegate object MethodBoolReturn(bool value);
	public delegate object MethodVector2Return(Vector2 value);
	public delegate object MethodVector3Return(Vector3 value);
	//=======================
	// Main
	//=======================
	[AddComponentMenu("")]
	public class Event : EventDetector{
		[EnumMask] public static EventDisabled disabled;
		[EnumMask] public static EventDebugScope debugScope;
		[EnumMask] public static EventDebug debug;
		public static Event instance;
		public static object all = "All";
		public static object global = "Global";
		public static EventListener emptyListener = new EventListener();
		public static Dictionary<object,Dictionary<string,EventListener>> unique = new Dictionary<object,Dictionary<string,EventListener>>();
		public static Dictionary<object,Dictionary<string,Dictionary<object,EventListener>>> cache = new Dictionary<object,Dictionary<string,Dictionary<object,EventListener>>>();
		public static List<EventListener> listeners = new List<EventListener>();
		public static Dictionary<object,List<string>> callers = new Dictionary<object,List<string>>();
		public static Dictionary<object,List<string>> active = new Dictionary<object,List<string>>();
		[NonSerialized] public static string lastCalled;
		public static FixedList<string> eventHistory = new FixedList<string>(15);
		public static List<EventListener> stack = new List<EventListener>();
		private bool setup;
		public static bool IsSetup(){return !Event.instance.IsNull() && Event.instance.setup;}
		public override void Awake(){
			this.setup = true;
			Event.instance = this;
			Event.callers.Clear();
			Event.cache.Clear();
			Event.listeners.RemoveAll(x=>x.name!="On Events Reset"&&(!x.permanent||x.occurrences==0));
			foreach(var listener in Event.listeners){
				var scope = Event.cache.AddNew(listener.target).AddNew(listener.name);
				scope[listener.method] = listener;
			}
			Event.Call("On Events Reset");
			base.Awake();
		}
		[ContextMenu("Reset All")]
		public void ResetAll(){
			Event.listeners.Clear();
			Event.cache.Clear();
			Event.callers.Clear();
			Event.unique.Clear();
			EventStepper.instances.Clear();
		}
		public static void Cleanup(){
			if(Application.isPlaying){return;}
			foreach(var cached in Event.cache.Copy()){
				foreach(var set in cached.Value.Copy()){
					foreach(var eventPair in set.Value.Copy()){
						var listener = eventPair.Value;
						Delegate method = (Delegate)listener.method;
						bool targetMissing = !listener.isStatic && listener.target.IsNull();
						bool methodMissing = !listener.isStatic && method.Target.IsNull();
						if(targetMissing || methodMissing || eventPair.Key.IsNull()){
							listener.Remove();
						}
					}
					if(set.Key.IsNull() || set.Value.Count < 1){
						Event.cache[cached.Key].Remove(set.Key);
					}
				}
				if(cached.Key.IsNull() || cached.Value.Count < 1){
					Event.cache.Remove(cached.Key);
				}
			}
			foreach(var listener in Event.listeners.Copy()){
				Delegate method = (Delegate)listener.method;
				bool targetMissing = !listener.isStatic && listener.target.IsNull();
				bool methodMissing = !listener.isStatic && method.Target.IsNull();
				if(targetMissing || methodMissing){
					listener.Remove();
				}
			}
			foreach(var item in Event.callers.Copy()){
				if(item.Key.IsNull()){
					Event.callers.Remove(item.Key);
				}
			}
		}
		public static object Verify(object target=null){
			if(target.IsNull()){target = Event.global;}
			return target;
		}
		public static object[] VerifyAll(params object[] targets){
			if(targets.Length < 1){targets = new object[1]{Event.global};}
			return targets;
		}
		public static void Empty(){}
		public static void Register(string name){Event.Register(name,Event.Verify());}
		public static void Register(string name,params object[] targets){
			if(Event.disabled.Has("Add")){return;}
			foreach(object target in targets){
				if(target.IsNull()){continue;}
				Event.callers.AddNew(target);
				Event.callers[target].AddNew(name);
			}
		}
		public static void AddStepper(string eventName,MethodStep method,IList collection,int passes=1){
			var stepper = new EventStepper(method,null,collection,passes);
			stepper.onComplete = ()=>Event.Remove(eventName,stepper.Step);
			Event.Add(eventName,stepper.Step).SetPermanent();
		}
		public static EventListener Add(string name,Method method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodObject method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodFull method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodString method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodInt method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodFloat method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodBool method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodVector2 method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodVector3 method,params object[] targets){return Event.Add(name,(object)method,-1,targets);}
		public static EventListener AddLimited(string name,Method method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodObject method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodFull method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodString method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodInt method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodFloat method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodBool method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodVector2 method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodVector3 method,int amount=1,params object[] targets){return Event.Add(name,(object)method,amount,targets);}
		public static EventListener Add(string name,object method,int amount,params object[] targets){
			bool delayed = false;
			bool systemReady = !Event.instance.IsNull() && Event.instance.setup;
			if(!systemReady){
				if(Event.debug.Has("Add")){
					Debug.Log("[Events] : System not ready.  Delaying event add -- " + Event.GetMethodName(method.As<Delegate>()) + " -- " + name);
				}
				delayed = true;
			}
			if(Event.disabled.Has("Add")){
				Debug.LogWarning("[Events] : Add attempted while Events disabled. " + name);
				return null;
			}
			targets = Event.VerifyAll(targets);
			var listener = Event.emptyListener;
			foreach(object current in targets){
				var target = current;
				if(target.IsNull()){continue;}
				if(Event.unique.ContainsKey(target) && Event.unique[target].ContainsKey(name)){
					listener = Event.unique[target][name];
					continue;
				}
				if(!Event.cache.AddNew(target).AddNew(name).ContainsKey(method)){
					listener = new EventListener();
					if(systemReady && Event.debug.Has("Add")){
						var info = (Delegate)method;
						Debug.Log("[Events] : Adding event -- " + Event.GetMethodName(info) + " -- " + name,target as UnityObject);
					}
					Event.listeners.Add(listener);
					Utility.DelayCall(Event.OnEventsChanged);
				}
				else{
					listener = Event.cache[target][name].AddNew(method);
				}
				if(delayed){
					listener.name = "On Events Reset";
					listener.method = (Method)(()=>{
						var newEvent = Event.Add(name,method,amount,target);
						newEvent.SetPermanent(listener.permanent);
						newEvent.SetUnique(listener.unique);
						listener.SetPermanent(false);
						listener.SetUnique(false);
					});
					listener.target = target = Event.global;
					listener.occurrences = 1;
				}
				else{
					listener.name = name;
					listener.method = method;
					listener.target = target;
					listener.occurrences = amount;
					listener.isStatic = ((Delegate)method).Target.IsNull();
				}
				Event.cache.AddNew(target).AddNew(listener.name)[listener.method] = listener;
				Event.cache.AddNew(Event.all).AddNew(listener.name)[listener.method] = listener;
			}
			return listener;
		}
		public static void OnEventsChanged(){Event.Call("On Events Changed");}
		public static void Remove(string name,Method method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodObject method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodFull method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodString method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodInt method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodFloat method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodBool method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodVector2 method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodVector3 method,params object[] targets){Event.Remove(name,(object)method,targets);}
		public static void Remove(string name,object method,params object[] targets){
			if(Event.disabled.Has("Add")){return;}
			targets = Event.VerifyAll(targets);
			foreach(var target in targets){
				var removals = Event.listeners.Where(x=>x.method==method && x.target==target && x.name==name).ToList();
				removals.ForEach(x=>x.Remove());
			}
			Utility.DelayCall(Event.OnEventsChanged);
		}
		public static void RemoveAll(string name,params object[] targets){
			foreach(var target in targets){
				Event.listeners.Where(x=>x.target==target&&x.name== name).ToList().ForEach(x=>x.Remove());
			}
		}
		public static void RemoveAll(params object[] targets){
			if(Event.disabled.Has("Add")){return;}
			targets = Event.VerifyAll(targets);
			foreach(var target in targets){
				var removals = Event.listeners.Where(x=>x.target==target || x.method.As<Delegate>().Target==target).ToList();
				removals.ForEach(x=>x.Remove());
				Event.cache.AddNew(target).SelectMany(x=>x.Value).Select(x=>x.Value).ToList().ForEach(x=>x.Remove());
				Event.cache.Remove(target);
			}
			Utility.DelayCall(Event.OnEventsChanged);
		}
		public static Dictionary<object,EventListener> Get(string name){return Event.Get(Event.global,name);}
		public static Dictionary<object,EventListener> Get(object target,string name){
			return Event.cache.AddNew(target).AddNew(name);
		}
		public static bool HasListeners(object target,string name="*"){
			target = Event.Verify(target);
			if(name == "*"){return Event.cache.ContainsKey(target);}
			return Event.cache.ContainsKey(target) && Event.cache[target].ContainsKey(name);
		}
		public static bool HasCallers(object target,string name="*"){
			target = Event.Verify(target);
			if(name == "*"){return Event.callers.ContainsKey(target);}
			return Event.callers.ContainsKey(target) && Event.callers[target].Contains(name);
		}
		public static void SetPause(string type,string name,object target){
			target = Event.Verify(target);
			if(Event.debug.Has("Pause")){
				string message = "[Events] : " + type + " event -- " + Event.GetTargetName(target) + " -- " + name;
				Debug.Log(message,target as UnityObject);
			}
			foreach(var item in Event.Get(target,name)){
				item.Value.paused = type == "Pausing";
			}
		}
		public static void Pause(string name,object target=null){Event.SetPause("Pausing",name,target);}
		public static void Resume(string name,object target=null){Event.SetPause("Resuming",name,target);}
		public static void AddHistory(string name){
			if(Event.debug.Has("History")){
				int lastIndex = Event.eventHistory.Count-1;
				if(lastIndex >= 0){
					string last = Event.eventHistory[lastIndex];
					string lastReal = last.Split("(")[0].Trim();
					if(lastReal == name){
						string value = last.Parse("(",")");
						int count = value.IsEmpty() ? 2 : value.ToInt() + 1;
						value = " (" + count.ToString() + ")";
						Event.eventHistory[lastIndex] = name + value;
						return;
					}
				}
				Event.eventHistory.Add(name);
			}
		}
		public static void Rest(string name,float seconds){Event.Rest(Event.global,name,seconds);}
		public static void Rest(object target,string name,float seconds){
			foreach(var item in Event.Get(target,name)){
				item.Value.Rest(seconds);
			}
		}
		public static void SetCooldown(string name,float seconds){Event.SetCooldown(Event.global,name,seconds);}
		public static void SetCooldown(object target,string name,float seconds){
			foreach(var item in Event.Get(target,name)){
				item.Value.SetCooldown(seconds);
			}
		}
		public static void DelayCall(string name,float delay=0.5f,params object[] values){
			Event.DelayCall(Event.global,"Global",name,delay,values);
		}
		public static void DelayCall(object key,string name,float delay=0.5f,params object[] values){
			Event.DelayCall(Event.global,key,name,delay,values);
		}
		public static void DelayCall(object target,object key,string name,float delay=0.5f,params object[] values){
			if(target.IsNull()){return;}
			Utility.DelayCall(key,()=>Event.Call(target,name,values),delay);
		}
		public static void Call(string name,params object[] values){
			if(Event.disabled.Has("Call")){return;}
			Event.Call(Event.Verify(),name,values);
		}
		public static void Call(object target,string name,params object[] values){
			if(Event.disabled.Has("Call")){return;}
			if(Event.active.AddNew(target).Contains(name)){return;}
			if(Event.stack.Count > 1000){
				Debug.LogWarning("[Events] : Event stack overflow.");
				Event.disabled = (EventDisabled)(-1);
				return;
			}
			target = Event.Verify(target);
			bool hasEvents = Event.HasListeners(target,name);
			var events = hasEvents ? Event.cache[target][name] : null;
			int count = hasEvents ? events.Count : 0;
			bool canDebug = Event.CanDebug(target,name,count);
			bool debugDeep = canDebug && Event.debug.Has("CallDeep");
			bool debugTime = canDebug && Event.debug.Has("CallTimer");
			float duration = Time.realtimeSinceStartup;
			if(hasEvents){
				Event.lastCalled = name;
				Event.active[target].Add(name);
				foreach(var item in events.Copy()){
					item.Value.Call(debugDeep,debugTime,values);
				}
				Event.lastCalled = "";
				Event.active[target].Remove(name);
			}
			if(debugTime && (!debugDeep || count < 1)){
				duration = Time.realtimeSinceStartup - duration;
				if(duration > 0.001f || Event.debug.Has("CallTimerZero")){
					string time = duration.ToString("F10").TrimRight("0",".").Trim() + " seconds.";
					string message = "[Events] : " + Event.GetTargetName(target) + " -- " + name + " -- " + count + " events -- " + time;
					Debug.Log(message,target as UnityObject);
				}
			}
		}
		public static void CallChildren(object target,string name,object[] values,bool self=false){
			if(Event.disabled.Has("Call")){return;}
			if(self){Event.Call(target,name,values);}
			if(target is GameObject){
				var gameObject = (GameObject)target;
				Transform[] children = Locate.GetObjectComponents<Transform>(gameObject);
				foreach(Transform transform in children){
					if(transform.gameObject == gameObject){continue;}
					Event.CallChildren(transform.gameObject,name,values,true);
				}
			}
		}
		public static void CallParents(object target,string name,object[] values,bool self=false){
			if(Event.disabled.Has("Call")){return;}
			if(self){Event.Call(target,name,values);}
			if(target is GameObject){
				var gameObject = (GameObject)target;
				Transform parent = gameObject.transform.parent;
				while(parent != null){
					Event.CallParents(parent.gameObject,name,values,true);
					parent = parent.parent;
				}
			}
		}
		public static void CallFamily(object target,string name,object[] values,bool self=false){
			if(Event.disabled.Has("Call")){return;}
			if(self){Event.Call(target,name,values);}
			Event.CallChildren(target,name,values);
			Event.CallParents(target,name,values);
		}
		//========================
		// Editor
		//========================
		public static string GetTargetName(object target){
			if(target.IsNull()){return "Null";}
			string targetName = "";
			if(target is string){targetName = target.ToString();}
			if(target is GameObject){targetName = ((GameObject)target).GetPath();}
			else if(target is Component){targetName = ((Component)target).GetPath();}
			else if(target.HasVariable("path")){targetName = target.GetVariable<string>("path");}
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
					targetName = GetTargetName(info.Target);
				}
				if(targetName.Contains("__")){targetName = "Anonymous";}
				if(methodName.Contains("__")){methodName = "Anonymous";}
				name = targetName + "." + methodName + "()";
			}
			return name;
		}
		public static bool CanDebug(object target,string name,int count){
			bool allowed = true;
			var debug = Event.debug;
			var scope = Event.debugScope;
			allowed = target == Event.global ? scope.Has("Global") : scope.Has("Scoped");
			allowed = allowed && (count > 0 || debug.HasAny("CallEmpty"));
			if(allowed && name.ContainsAny("On Update","On Late Update","On Fixed Update","On Editor Update","On GUI","On Camera","On Undo Flushing")){
				allowed = debug.Has("CallUpdate");
			}
			if(allowed && !debug.Has("CallTimer") && debug.HasAny("Call","CallUpdate","CallDeep","CallEmpty")){
				string message = "[Events] : Calling " + name + " -- " + count + " events -- " + Event.GetTargetName(target);
				Debug.Log(message,target as UnityObject);
			}
			return allowed;
		}
		public static void Clean(string ignoreName="",object target=null,object targetMethod=null){
			foreach(var eventListener in Event.listeners){
				string eventName = eventListener.name;
				object eventTarget = eventListener.target;
				object eventMethod = eventListener.method;
				bool duplicate = eventName != ignoreName && eventTarget == target && eventMethod.Equals(targetMethod);
				bool invalid = eventTarget.IsNull() || eventMethod.IsNull() || (!eventListener.isStatic && ((Delegate)eventMethod).Target.IsNull());
				if(duplicate || invalid){
					Utility.DelayCall(()=>Event.listeners.Remove(eventListener));
					if(Event.debug.Has("Remove")){
						string messageType = eventMethod.IsNull() ? "empty method" : "duplicate method";
						string message = "[Events] Removing " + messageType  + " from -- " + eventTarget + "/" + eventName;
						Debug.Log(message,target as UnityObject);
					}
				}
			}
			foreach(var current in Event.callers){
				object scope = current.Key;
				if(scope.IsNull()){
					Utility.DelayCall(()=>Event.callers.Remove(scope));
				}
			}
		}
		public static List<string> GetEventNames(string type,object target=null){
			Utility.EditorCall(()=>Event.Clean());
			target = Event.Verify(target);
			if(type.Contains("Listen",true)){
				return Event.listeners.ToList().FindAll(x=>x.target==target).Select(x=>x.name).ToList();
			}
			if(Event.callers.ContainsKey(target)){
				return Event.callers[target];
			}
			return new List<string>();
		}
	}
	public static class ObjectEventExtensions{
		public static void RegisterEvent(this object current,string name,params object[] values){
			if(current.IsNull()){return;}
			Event.Register(name,current);
		}
		public static EventListener AddEvent(this object current,string name,object method,int amount=-1){
			if(current.IsNull()){return Event.emptyListener;}
			return Event.Add(name,method,amount,current);
		}
		public static void RemoveEvent(this object current,string name,object method){
			if(current.IsNull()){return;}
			Event.Remove(name,method,current);
		}
		public static void RemoveAllEvents(this object current,string name,object method){
			if(current.IsNull()){return;}
			Event.RemoveAll(current);
		}
		public static void DelayEvent(this object current,string name,float delay=0.5f,params object[] values){
			Event.DelayCall(current,current,name,delay,values);
		}
		public static void DelayEvent(this object current,object key,string name,float delay=0.5f,params object[] values){
			Event.DelayCall(current,key,name,delay,values);
		}
		public static void RestEvent(this object current,string name,float seconds){
			if(current.IsNull()){return;}
			Event.Rest(current,name,seconds);
		}
		public static void CooldownEvent(this object current,string name,float seconds){
			if(current.IsNull()){return;}
			Event.SetCooldown(current,name,seconds);
		}
		public static void CallEvent(this object current,string name,params object[] values){
			if(current.IsNull()){return;}
			Event.Call(current,name,values);
		}
		public static void CallEventChildren(this object current,string name,bool self=true,params object[] values){
			if(current.IsNull()){return;}
			Event.CallChildren(current,name,values,self);
		}
		public static void CallEventParents(this object current,string name,bool self=true,params object[] values){
			if(current.IsNull()){return;}
			Event.CallParents(current,name,values,self);
		}
		public static void CallEventFamily(this object current,string name,bool self=true,params object[] values){
			if(current.IsNull()){return;}
			Event.CallFamily(current,name,values,self);
		}
	}
}