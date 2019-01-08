namespace Zios.Events{
	using Zios.Unity.Supports.Singleton;
	using Zios.Unity.SystemAttributes;
	[InitializeOnLoad]
	public class EventsManager : Singleton{
		public static EventsManager singleton;
		[EnumMask] public EventDisabled disabled;
		[EnumMask] public EventDebugScope debugScope;
		[EnumMask] public EventDebug debug;
		public static EventsManager Get(){
			EventsManager.singleton = EventsManager.singleton ?? Singleton.Get<EventsManager>();
			return EventsManager.singleton;
		}
		public void OnEnable(){
			this.Update();
			this.SetupHooks();
		}
		public void Update(){
			Events.disabled = this.disabled;
			Events.debugScope = this.debugScope;
			Events.debug = this.debug;
		}
	}
}