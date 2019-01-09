using UnityEditor;
namespace Zios.Unity.Editor.Locate.LocateEvents{
	using Zios.Events;
	using Zios.Unity.Locate;
	using Zios.Unity.Locate.LocateEvents;
	using Zios.Unity.Proxy;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	[InitializeOnLoad]
	public static class LocateEventsEditor{
		static LocateEventsEditor(){
			if(!Proxy.IsPlaying()){
				//Events.Add("On Application Quit",LocateEvents.SetDirty);
				Events.Add("On Hierarchy Changed",LocateEvents.SetDirty).SetPermanent();
				Events.Add("On Asset Changed",()=>Locate.assets.Clear()).SetPermanent();
			}
		}
	}
}