//addon Zios.Unity.Call;
namespace Zios.Unity.Call.Events{
	using Zios.Events;
	using Zios.Shortcuts;
	using Zios.Unity.Call;
	using Zios.Unity.Proxy;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.SystemAttributes;
	using Zios.Unity.Time;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	//asm Zios.Unity.Editor.Events;
	[InitializeOnLoad]
	public static class CallEvents{
		private static float sceneCheck;
		static CallEvents(){
			Events.Add("On Late Update",(Method)Call.CheckDelayed);
			Events.Add("On Late Update",(Method)CallEvents.CheckLoaded);
			Events.Add("On Editor Update",()=>CallEvents.CheckLoaded(true));
			Events.Add("On Editor Update",()=>Call.CheckDelayed(true));
		}
		public static void CheckLoaded(){CallEvents.CheckLoaded(false);}
		public static void CheckLoaded(bool editor){
			if(editor && Proxy.IsPlaying()){return;}
			if(!editor && !Proxy.IsPlaying()){return;}
			if(Time.Get() < 0.5 && CallEvents.sceneCheck == 0){
				var term = editor ? " Editor" : "";
				Events.Call("On" + term + " Scene Loaded");
				CallEvents.sceneCheck = 1;
			}
			if(Time.Get() > CallEvents.sceneCheck){
				CallEvents.sceneCheck = 0;
			}
		}
	}
}