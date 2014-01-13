using UnityEngine;
using System;
using System.Collections.Generic;
public static class Events{
	private static Dictionary<object,Dictionary<string,object>> objectEvents = new Dictionary<object,Dictionary<string,object>>();
	private static Dictionary<string,List<object>> events = new Dictionary<string,List<object>>();
	//private static Dictionary<string,List<MethodFull>> fullEvents = new Dictionary<string,List<MethodFull>>();
	public static void Add(string name,Method method){
		if(!Events.events.ContainsKey(name)){
			Events.events[name] = new List<object>();
		}
		Events.events[name].Add(method);
	}
	/*public static void Add(string name,MethodFull method){
		if(!Events.fullEvents.ContainsKey(name)){
			Events.fullEvents[name] = new List<MethodFull>();
		}
		Events.fullEvents[name].Add(method);
	}*/
	public static void Call(string name){
		if(Events.events.ContainsKey(name)){
			foreach(object callback in Events.events[name]){
				if(callback is Method){
					((Method)callback)();
				}
			}
		}
		/*if(Events.fullEvents.Contains(name)){
			foreach(MethodFull method in Events.fullEvents[name]){
				method();
			}
		}*/
	}
	public static void Call(string name,object target){
		if(Events.objectEvents.ContainsKey(target)){
			if(Events.objectEvents[target].ContainsKey(name)){
				object callback = Events.objectEvents[target][name];
				if(callback is Method){
					((Method)callback)();
				}
			}
		}
	}
}