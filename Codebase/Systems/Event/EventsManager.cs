using System;
using UnityEngine;
namespace Zios.Event{
	[InitializeOnLoad]
	public class EventsManager : ScriptableObject{
		public static EventsManager singleton;
		[EnumMask] public EventDisabled disabled;
		[EnumMask] public EventDebugScope debugScope;
		[EnumMask] public EventDebug debug;
		public static EventsManager Get(){
			EventsManager.singleton = EventsManager.singleton ?? Utility.GetSingleton<EventsManager>();
			return EventsManager.singleton;
		}
	}
}