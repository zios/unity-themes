#pragma warning disable 0618
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Event{
	using Containers;
	//=======================
	// Enumerations
	//=======================
	[Flags][Serializable]
	public enum EventDisabled : int{
		Add        = 0x001,
		Call       = 0x002,
	}
	[Flags][Serializable]
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
	[Serializable]
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
	public delegate void MethodObjects(object[] values);
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
	[InitializeOnLoad]
	public static class Events{
		[EnumMask] public static EventDisabled disabled;
		[EnumMask] public static EventDebugScope debugScope;
		[EnumMask] public static EventDebug debug;
		public static object all = "All";
		public static object global = "Global";
		public static EventListener emptyListener = new EventListener();
		public static Dictionary<object,Dictionary<string,EventListener>> unique = new Dictionary<object,Dictionary<string,EventListener>>();
		public static Dictionary<object,Dictionary<string,Dictionary<object,EventListener>>> cache = new Dictionary<object,Dictionary<string,Dictionary<object,EventListener>>>();
		public static List<EventListener> listeners = new List<EventListener>();
		public static Dictionary<object,List<string>> callers = new Dictionary<object,List<string>>();
		public static Dictionary<object,List<string>> active = new Dictionary<object,List<string>>();
		public static string lastCalled;
		public static FixedList<string> eventHistory = new FixedList<string>(15);
		public static List<EventListener> stack = new List<EventListener>();
		static Events(){
			Events.callers.Clear();
			Events.cache.Clear();
			Events.listeners.RemoveAll(x=>x.name!="On Events Reset"&&(!x.permanent||x.occurrences==0));
			#if UNITY_EDITOR
			Action Repair = ()=>{
				var main = Locate.GetScenePath("@Main");
				if(main.GetComponent<EventDetector>().IsNull()){
					main.AddComponent<EventDetector>();
					main.hideFlags = HideFlags.HideInHierarchy;
				}
				#if UNITY_THEMES
				main.hideFlags = HideFlags.HideAndDontSave;
				#endif
			};
			Repair();
			Events.Add("On Destroy",()=>Utility.DelayCall(Repair));
			#endif
			foreach(var listener in Events.listeners){
				var scope = Events.cache.AddNew(listener.target).AddNew(listener.name);
				scope[listener.method] = listener;
			}
			Events.Call("On Events Reset");
		}
		public static void Cleanup(){
			if(Application.isPlaying){return;}
			foreach(var cached in Events.cache.Copy()){
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
						Events.cache[cached.Key].Remove(set.Key);
					}
				}
				if(cached.Key.IsNull() || cached.Value.Count < 1){
					Events.cache.Remove(cached.Key);
				}
			}
			foreach(var listener in Events.listeners.Copy()){
				Delegate method = (Delegate)listener.method;
				bool targetMissing = !listener.isStatic && listener.target.IsNull();
				bool methodMissing = !listener.isStatic && method.Target.IsNull();
				if(targetMissing || methodMissing){
					listener.Remove();
				}
			}
			foreach(var item in Events.callers.Copy()){
				if(item.Key.IsNull()){
					Events.callers.Remove(item.Key);
				}
			}
		}
		public static object Verify(object target=null){
			if(target.IsNull()){target = Events.global;}
			return target;
		}
		public static object[] VerifyAll(params object[] targets){
			if(targets.Length < 1){targets = new object[1]{Events.global};}
			return targets;
		}
		public static void Empty(){}
		public static void Register(string name){Events.Register(name,Events.Verify());}
		public static void Register(string name,params object[] targets){
			if(Events.HasDisabled("Add")){return;}
			foreach(object target in targets){
				if(target.IsNull()){continue;}
				Events.callers.AddNew(target);
				Events.callers[target].AddNew(name);
			}
		}
		public static void AddStepper(string eventName,MethodStep method,IList collection,int passes=1){
			var stepper = new EventStepper(method,null,collection,passes);
			stepper.onComplete = ()=>Events.Remove(eventName,stepper.Step);
			Events.Add(eventName,stepper.Step).SetPermanent();
		}
		public static EventListener Add(string name,Method method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodObject method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodObjects method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodString method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodInt method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodFloat method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodBool method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodVector2 method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add(string name,MethodVector3 method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener Add<Type>(string name,Type method,params object[] targets){return Events.Add(name,(object)method,-1,targets);}
		public static EventListener AddLimited(string name,Method method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodObject method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodObjects method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodString method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodInt method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodFloat method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodBool method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodVector2 method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener AddLimited(string name,MethodVector3 method,int amount=1,params object[] targets){return Events.Add(name,(object)method,amount,targets);}
		public static EventListener Add(string name,object method,int amount,params object[] targets){
			if(Events.HasDisabled("Add")){
				Debug.LogWarning("[Events] : Add attempted while Events disabled. " + name);
				return null;
			}
			targets = Events.VerifyAll(targets);
			var listener = Events.emptyListener;
			foreach(object current in targets){
				var target = current;
				if(target.IsNull()){continue;}
				if(Events.unique.ContainsKey(target) && Events.unique[target].ContainsKey(name)){
					listener = Events.unique[target][name];
					continue;
				}
				if(!Events.cache.AddNew(target).AddNew(name).ContainsKey(method)){
					listener = new EventListener();
					if(Events.HasDebug("Add")){
						var info = (Delegate)method;
						Debug.Log("[Events] : Adding event -- " + Events.GetMethodName(info) + " -- " + name,target as UnityObject);
					}
					Events.listeners.Add(listener);
					Utility.DelayCall(Events.OnEventsChanged);
				}
				else{
					listener = Events.cache[target][name].AddNew(method);
				}
				listener.name = name;
				listener.method = method;
				listener.target = target;
				listener.occurrences = amount;
				listener.isStatic = ((Delegate)method).Target.IsNull();
				Events.cache.AddNew(target).AddNew(listener.name)[listener.method] = listener;
				Events.cache.AddNew(Events.all).AddNew(listener.name)[listener.method] = listener;
			}
			return listener;
		}
		public static void OnEventsChanged(){Events.Call("On Events Changed");}
		public static void Remove(string name,Method method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodObject method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodObjects method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodString method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodInt method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodFloat method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodBool method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodVector2 method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,MethodVector3 method,params object[] targets){Events.Remove(name,(object)method,targets);}
		public static void Remove(string name,object method,params object[] targets){
			if(Events.HasDisabled("Add")){return;}
			targets = Events.VerifyAll(targets);
			foreach(var target in targets){
				var removals = Events.listeners.Where(x=>x.method==method && x.target==target && x.name==name).ToList();
				removals.ForEach(x=>x.Remove());
			}
			Utility.DelayCall(Events.OnEventsChanged);
		}
		public static void RemoveAll(string name,params object[] targets){
			foreach(var target in targets){
				Events.listeners.Where(x=>x.target==target&&x.name== name).ToList().ForEach(x=>x.Remove());
			}
		}
		public static void RemoveAll(params object[] targets){
			if(Events.HasDisabled("Add")){return;}
			targets = Events.VerifyAll(targets);
			foreach(var target in targets){
				var removals = Events.listeners.Where(x=>x.target==target || x.method.As<Delegate>().Target==target).ToList();
				removals.ForEach(x=>x.Remove());
				Events.cache.AddNew(target).SelectMany(x=>x.Value).Select(x=>x.Value).ToList().ForEach(x=>x.Remove());
				Events.cache.Remove(target);
			}
			Utility.DelayCall(Events.OnEventsChanged);
		}
		public static Dictionary<object,EventListener> Get(string name){return Events.Get(Events.global,name);}
		public static Dictionary<object,EventListener> Get(object target,string name){
			return Events.cache.AddNew(target).AddNew(name);
		}
		public static bool HasListeners(object target,string name="*"){
			target = Events.Verify(target);
			if(name == "*"){return Events.cache.ContainsKey(target);}
			return Events.cache.ContainsKey(target) && Events.cache[target].ContainsKey(name);
		}
		public static bool HasCallers(object target,string name="*"){
			target = Events.Verify(target);
			if(name == "*"){return Events.callers.ContainsKey(target);}
			return Events.callers.ContainsKey(target) && Events.callers[target].Contains(name);
		}
		public static void SetPause(string type,string name,object target){
			target = Events.Verify(target);
			if(Events.HasDebug("Pause")){
				string message = "[Events] : " + type + " event -- " + Events.GetTargetName(target) + " -- " + name;
				Debug.Log(message,target as UnityObject);
			}
			foreach(var item in Events.Get(target,name)){
				item.Value.paused = type == "Pausing";
			}
		}
		public static void Pause(string name,object target=null){Events.SetPause("Pausing",name,target);}
		public static void Resume(string name,object target=null){Events.SetPause("Resuming",name,target);}
		public static void AddHistory(string name){
			if(Events.HasDebug("History")){
				int lastIndex = Events.eventHistory.Count-1;
				if(lastIndex >= 0){
					string last = Events.eventHistory[lastIndex];
					string lastReal = last.Split("(")[0].Trim();
					if(lastReal == name){
						string value = last.Parse("(",")");
						int count = value.IsEmpty() ? 2 : value.ToInt() + 1;
						value = " (" + count.ToString() + ")";
						Events.eventHistory[lastIndex] = name + value;
						return;
					}
				}
				Events.eventHistory.Add(name);
			}
		}
		public static void Rest(string name,float seconds){Events.Rest(Events.global,name,seconds);}
		public static void Rest(object target,string name,float seconds){
			foreach(var item in Events.Get(target,name)){
				item.Value.Rest(seconds);
			}
		}
		public static void SetCooldown(string name,float seconds){Events.SetCooldown(Events.global,name,seconds);}
		public static void SetCooldown(object target,string name,float seconds){
			foreach(var item in Events.Get(target,name)){
				item.Value.SetCooldown(seconds);
			}
		}
		public static void DelayCall(string name,float delay=0.5f,params object[] values){
			Events.DelayCall(Events.global,"Global",name,delay,values);
		}
		public static void DelayCall(object key,string name,float delay=0.5f,params object[] values){
			Events.DelayCall(Events.global,key,name,delay,values);
		}
		public static void DelayCall(object target,object key,string name,float delay=0.5f,params object[] values){
			if(target.IsNull()){return;}
			Utility.DelayCall(key,()=>Events.Call(target,name,values),delay);
		}
		public static void Call(string name,params object[] values){
			if(Events.HasDisabled("Call")){return;}
			Events.Call(Events.Verify(),name,values);
		}
		public static void Call(object target,string name,params object[] values){
			if(Events.HasDisabled("Call")){return;}
			if(Events.active.AddNew(target).Contains(name)){return;}
			if(Events.stack.Count > 1000){
				Debug.LogWarning("[Events] : Event stack overflow.");
				Events.disabled = (EventDisabled)(-1);
				return;
			}
			target = Events.Verify(target);
			bool hasEvents = Events.HasListeners(target,name);
			var events = hasEvents ? Events.cache[target][name] : null;
			int count = hasEvents ? events.Count : 0;
			bool canDebug = Events.CanDebug(target,name,count);
			bool debugDeep = canDebug && Events.HasDebug("CallDeep");
			bool debugTime = canDebug && Events.HasDebug("CallTimer");
			float duration = Time.realtimeSinceStartup;
			if(hasEvents){
				Events.lastCalled = name;
				Events.active[target].Add(name);
				foreach(var item in events.Copy()){
					item.Value.Call(debugDeep,debugTime,values);
				}
				Events.lastCalled = "";
				Events.active[target].Remove(name);
			}
			if(debugTime && (!debugDeep || count < 1)){
				duration = Time.realtimeSinceStartup - duration;
				if(duration > 0.001f || Events.HasDebug("CallTimerZero")){
					string time = duration.ToString("F10").TrimRight("0",".").Trim() + " seconds.";
					string message = "[Events] : " + Events.GetTargetName(target) + " -- " + name + " -- " + count + " events -- " + time;
					Debug.Log(message,target as UnityObject);
				}
			}
		}
		public static void CallChildren(object target,string name,object[] values,bool self=false){
			if(Events.HasDisabled("Call")){return;}
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
			if(Events.HasDisabled("Call")){return;}
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
			if(Events.HasDisabled("Call")){return;}
			if(self){Events.Call(target,name,values);}
			Events.CallChildren(target,name,values);
			Events.CallParents(target,name,values);
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
			var debug = Events.debug;
			var scope = Events.debugScope;
			allowed = target == Events.global ? scope.Has("Global") : scope.Has("Scoped");
			allowed = allowed && (count > 0 || debug.HasAny("CallEmpty"));
			if(allowed && name.ContainsAny("On Update","On Late Update","On Fixed Update","On Editor Update","On GUI","On Camera","On Undo Flushing")){
				allowed = debug.Has("CallUpdate");
			}
			if(allowed && !debug.Has("CallTimer") && debug.HasAny("Call","CallUpdate","CallDeep","CallEmpty")){
				string message = "[Events] : Calling " + name + " -- " + count + " events -- " + Events.GetTargetName(target);
				Debug.Log(message,target as UnityObject);
			}
			return allowed;
		}
		public static bool HasDisabled(string term){
			return Events.disabled.Has(term);
		}
		public static bool HasDebug(string term){
			return Events.debug.Has(term);
		}
		public static void Clean(string ignoreName="",object target=null,object targetMethod=null){
			foreach(var eventListener in Events.listeners){
				string eventName = eventListener.name;
				object eventTarget = eventListener.target;
				object eventMethod = eventListener.method;
				bool duplicate = eventName != ignoreName && eventTarget == target && eventMethod.Equals(targetMethod);
				bool invalid = eventTarget.IsNull() || eventMethod.IsNull() || (!eventListener.isStatic && ((Delegate)eventMethod).Target.IsNull());
				if(duplicate || invalid){
					Utility.DelayCall(()=>Events.listeners.Remove(eventListener));
					if(Events.HasDebug("Remove")){
						string messageType = eventMethod.IsNull() ? "empty method" : "duplicate method";
						string message = "[Events] Removing " + messageType  + " from -- " + eventTarget + "/" + eventName;
						Debug.Log(message,target as UnityObject);
					}
				}
			}
			foreach(var current in Events.callers){
				object scope = current.Key;
				if(scope.IsNull()){
					Utility.DelayCall(()=>Events.callers.Remove(scope));
				}
			}
		}
		public static List<string> GetEventNames(string type,object target=null){
			Utility.EditorCall(()=>Events.Clean());
			target = Events.Verify(target);
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
			if(current.IsNull()){return;}
			Events.Register(name,current);
		}
		public static EventListener AddEvent(this object current,string name,object method,int amount=-1){
			if(current.IsNull()){return Events.emptyListener;}
			return Events.Add(name,method,amount,current);
		}
		public static void RemoveEvent(this object current,string name,object method){
			if(current.IsNull()){return;}
			Events.Remove(name,method,current);
		}
		public static void RemoveAllEvents(this object current,string name,object method){
			if(current.IsNull()){return;}
			Events.RemoveAll(current);
		}
		public static void DelayEvent(this object current,string name,float delay=0.5f,params object[] values){
			Events.DelayCall(current,current,name,delay,values);
		}
		public static void DelayEvent(this object current,object key,string name,float delay=0.5f,params object[] values){
			Events.DelayCall(current,key,name,delay,values);
		}
		public static void RestEvent(this object current,string name,float seconds){
			if(current.IsNull()){return;}
			Events.Rest(current,name,seconds);
		}
		public static void CooldownEvent(this object current,string name,float seconds){
			if(current.IsNull()){return;}
			Events.SetCooldown(current,name,seconds);
		}
		public static void CallEvent(this object current,string name,params object[] values){
			if(current.IsNull()){return;}
			Events.Call(current,name,values);
		}
		public static void CallEventChildren(this object current,string name,bool self=true,params object[] values){
			if(current.IsNull()){return;}
			Events.CallChildren(current,name,values,self);
		}
		public static void CallEventParents(this object current,string name,bool self=true,params object[] values){
			if(current.IsNull()){return;}
			Events.CallParents(current,name,values,self);
		}
		public static void CallEventFamily(this object current,string name,bool self=true,params object[] values){
			if(current.IsNull()){return;}
			Events.CallFamily(current,name,values,self);
		}
	}
}