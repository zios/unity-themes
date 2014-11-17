using UnityEngine;
using System;
using System.Collections.Generic;
public static class ComponentExtension{
	public static string GetAlias(this Component current){
		if(current.HasVariable("alias")){
			return current.GetVariable<string>("alias");
		}
		return current.GetType().ToString();
	}
	public static string GetPath(this Component current){	
		return current.gameObject.GetPath();
	}
	public static bool IsPrefab(this Component current){
		return current.gameObject.IsPrefab();
	}
	//====================
	// Interface
	//====================
	public static Component[] GetComponentsByInterface<T>(this Component current) where T : Component{
		return current.gameObject.GetComponentsByInterface<T>();
	}
	public static T GetComponent<T>(this Component current,bool includeInactive) where T : Component{
		return current.gameObject.GetComponent<T>(includeInactive);
	}
	public static T[] GetComponents<T>(this Component current,bool includeInactive) where T : Component{
		return current.gameObject.GetComponents<T>(includeInactive);
	}
	public static T GetComponentInParent<T>(this Component current,bool includeInactive) where T : Component{
		return current.gameObject.GetComponentInParent<T>(includeInactive);
	}
	public static T GetComponentInChildren<T>(this Component current,bool includeInactive) where T : Component{
		return current.gameObject.GetComponentInChildren<T>(includeInactive);
	}
}