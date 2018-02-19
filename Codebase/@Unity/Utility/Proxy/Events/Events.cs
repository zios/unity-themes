namespace Zios.Unity.UnityEvents{
	using Zios.Events;
	using Zios.Shortcuts;
	using Zios.Unity.Call;
	using Zios.Unity.Proxy;
	using Zios.Unity.Time;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	public static class UnityEvents{
		private static float sceneCheck;
		static UnityEvents(){
			Proxy.busyMethods.Add(()=>EventDetector.loading);
			Events.Add("On Late Update",(Method)UnityEvents.CheckLoaded);
			Events.Add("On Late Update",(Method)Call.CheckDelayed);
		}
		public static void CheckLoaded(){UnityEvents.CheckLoaded(false);}
		public static void CheckLoaded(bool editor){
			if(editor && Proxy.IsPlaying()){return;}
			if(!editor && !Proxy.IsPlaying()){return;}
			if(Time.Get() < 0.5 && UnityEvents.sceneCheck == 0){
				var term = editor ? " Editor" : "";
				Events.Call("On" + term + " Scene Loaded");
				UnityEvents.sceneCheck = 1;
			}
			if(Time.Get() > UnityEvents.sceneCheck){
				UnityEvents.sceneCheck = 0;
			}
		}
	}
}