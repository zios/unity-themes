using UnityEngine;
namespace Zios.Unity.Extensions{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	public static class ComponentExtension{
		public static GameObject GetParent(this Component current){
			if(current.IsNull()){return null;}
			return current.gameObject.GetParent();
		}
		public static string GetPath(this Component current,bool includeSelf=true){
			if(current.IsNull() || current.gameObject.IsNull()){return "Null";}
			string path = current.gameObject.GetPath();
			if(includeSelf){path += current.GetAlias();}
			return path;
		}
		public static bool IsEnabled(this Component current){
			bool enabled = !current.IsNull() && current.gameObject.activeInHierarchy;
			if(current is MonoBehaviour){enabled = enabled && current.As<MonoBehaviour>().enabled;}
			return enabled;
		}
		//====================
		// Interface
		//====================
		public static Component[] GetComponentsByInterface<T>(this Component current) where T : Component{
			if(current.IsNull()){return new Component[0];}
			return current.gameObject.GetComponentsByInterface<T>();
		}
		public static T GetComponent<T>(this Component current,bool includeInactive) where T : Component{
			if(current.IsNull()){return null;}
			return current.gameObject.GetComponent<T>(includeInactive);
		}
		public static T[] GetComponents<T>(this Component current,bool includeInactive) where T : Component{
			if(current.IsNull()){return new T[0];}
			return current.gameObject.GetComponents<T>(includeInactive);
		}
		public static T GetComponentInParent<T>(this Component current,bool includeInactive) where T : Component{
			if(current.IsNull()){return null;}
			return current.gameObject.GetComponentInParent<T>(includeInactive);
		}
		public static T GetComponentInChildren<T>(this Component current,bool includeInactive) where T : Component{
			if(current.IsNull()){return null;}
			return current.gameObject.GetComponentInChildren<T>(includeInactive);
		}
	}
}