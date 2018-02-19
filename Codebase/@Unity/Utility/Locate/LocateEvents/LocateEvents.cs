using System.Linq;
using UnityEngine;
namespace Zios.Unity.Locate.LocateEvents{
	using Zios.Events;
	using Zios.Shortcuts;
	using Zios.Unity.Locate;
	//asm Zios.Unity.Shortcuts;
	public static class LocateEvents{
		private static Component[] allComponents = new Component[0];
		static LocateEvents(){
			Events.Add("On Level Was Loaded",(Method)LocateEvents.SetDirty).SetPermanent();
			Events.Register("On Components Changed");
		}
		public static void BuildComponents(){
			var components = Resources.FindObjectsOfTypeAll<Component>();
			if(components.Length != LocateEvents.allComponents.Count() && !LocateEvents.allComponents.SequenceEqual(components)){
				if(Locate.setup){Events.Call("On Components Changed");}
				LocateEvents.allComponents = components;
			}
		}
		public static void SetDirty(){
			LocateEvents.BuildComponents();
			Locate.SetDirty();
		}
	}
}