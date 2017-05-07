using System;
using UnityEngine;
namespace Zios.Event{
	[InitializeOnLoad]
	public class EventsManager : Singleton{
		public static EventsManager singleton;
		[EnumMask] public EventDisabled disabled;
		[EnumMask] public EventDebugScope debugScope;
		[EnumMask] public EventDebug debug;
		public static EventsManager Get()
		{
			EventsManager.singleton = EventsManager.singleton ?? Utility.GetSingleton<EventsManager>();
			return EventsManager.singleton;
		}
		public void OnEnable(){this.Update();}
		public void Update(){
			Events.disabled = this.disabled;
			Events.debugScope = this.debugScope;
			Events.debug = this.debug;
		}
	}
}