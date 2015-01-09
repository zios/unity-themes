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
	public static void Move(this Component current,int amount){
		Utility.DisconnectPrefabInstance(current);
		while(amount != 0){
			if(amount > 0){
				Utility.MoveComponentDown(current);
				amount -= 1;
			}
			if(amount < 0){
				Utility.MoveComponentUp(current);
				amount += 1;
			}
		}
	}
	public static void MoveUp(this Component current){
		Component[] components = current.GetComponents<Component>();
		int position = components.IndexOf(current);
		int amount = 1;
		if(position != 0){
			while(components[position-1].hideFlags.Contains(HideFlags.HideInInspector)){
				position -= 1;
				amount += 1;
			}
		}
		current.Move(-amount);
	}
	public static void MoveDown(this Component current){
		Component[] components = current.GetComponents<Component>();
		int position = components.IndexOf(current);
		int amount = 1;
		if(position < components.Length-1){
			while(components[position+1].hideFlags.Contains(HideFlags.HideInInspector)){
				position += 1;
				amount += 1;
			}
		}
		current.Move(amount);
	}
	public static void MoveToTop(this Component current){
		Utility.DisconnectPrefabInstance(current);
		Component[] components = current.GetComponents<Component>();
		int position = components.IndexOf(current);
		current.Move(-position);
	}
	public static void MoveToBottom(this Component current){
		Utility.DisconnectPrefabInstance(current);
		Component[] components = current.GetComponents<Component>();
		int position = components.IndexOf(current);
		current.Move(components.Length-position);
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