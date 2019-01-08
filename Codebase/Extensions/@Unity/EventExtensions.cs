using UnityEngine;
namespace Zios{
	public static class EventExtension{
		public static bool IsUseful(this UnityEngine.Event current){
			if(current.type == EventType.ScrollWheel){return false;}
			if(current.type == EventType.Ignore){return false;}
			if(current.type == EventType.Used){return false;}
			return true;
		}
	}
}