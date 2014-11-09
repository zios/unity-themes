using UnityEngine;
using System;
using System.Collections.Generic;
public static class GameObjectExtension{
	//====================
	// Interface
	//====================
	public static Component[] GetComponentsByInterface<T>(this GameObject current){
		List<Component> results = new List<Component>();
		Component[] items = current.GetComponentsInChildren<Component>(true);
		foreach(Component item in items){
			if(item.GetType().IsAssignableFrom(typeof(T))){
				results.Add(item);
			}
		}
		return results.ToArray();
	}
	//====================
	// Layers / Tags
	//====================
	public static void ReplaceLayer(this GameObject current,string search,string replace){
		int layer = LayerMask.NameToLayer(replace);
		foreach(GameObject item in current.GetByLayer(search)){
			item.layer = layer;
		}
	}
	public static void ReplaceTag(this GameObject current,string search,string replace){
		foreach(GameObject item in current.GetByTag(search)){
			item.tag = replace;
		}
	}
	public static GameObject[] GetByLayer(this GameObject current,string search){
		int layer = LayerMask.NameToLayer(search);
		List<GameObject> results = new List<GameObject>();
		Transform[] children = current.GetComponentsInChildren<Transform>(true);
		foreach(Transform child in children){
			if(child.gameObject.layer == layer){
				results.Add(child.gameObject);
			}
		}
		return results.ToArray();
	}
	public static GameObject[] GetByTag(this GameObject current,string search){
		List<GameObject> results = new List<GameObject>();
		Transform[] children = current.GetComponentsInChildren<Transform>(true);
		foreach(Transform child in children){
			if(child.gameObject.tag == search){
				results.Add(child.gameObject);
			}
		}
		return results.ToArray();
	}
	public static void SetAllTags(this GameObject current,string name){
		Transform[] children = current.GetComponentsInChildren<Transform>(true);
		foreach(Transform child in children){
			child.gameObject.tag = name;
		}
	}
	public static void SetAllLayers(this GameObject current,string name){
		int layer = LayerMask.NameToLayer(name);
		Transform[] children = current.GetComponentsInChildren<Transform>(true);
		foreach(Transform child in children){
			child.gameObject.layer = layer;
		}
	}
	public static void SetLayer(this GameObject current,string name){
		int layer = LayerMask.NameToLayer(name);
		current.layer = layer;
	}
	//====================
	// Collisions
	//====================
	public static void ToggleAllCollisions(this GameObject current,bool state){
		current.ToggleComponents(state,true,typeof(Collider));
	}
	public static void ToggleAllTriggers(this GameObject current,bool state){
		Collider[] colliders = current.GetComponentsInChildren<Collider>();
		foreach(Collider collider in colliders){
			collider.isTrigger = state;
		}
	}
	public static void ToggleIgnoreCollisions(this GameObject current,GameObject target,bool state){
		var colliders = current.GetComponentsInChildren<Collider>();
		var targetColliders = target.GetComponentsInChildren<Collider>();
		foreach(Collider collider in colliders){
			if(!collider.enabled){continue;}
			foreach(Collider targetCollider in targetColliders){
				if(collider == targetCollider){continue;}
				if(!targetCollider.enabled){continue;}
				Physics.IgnoreCollision(collider,targetCollider,state);
			}
		}
	}
	public static void IgnoreAllCollisions(this GameObject current,GameObject target){
		current.ToggleIgnoreCollisions(target,true);
	}
	public static void UnignoreAllCollisions(this GameObject current,GameObject target){
		current.ToggleIgnoreCollisions(target,false);
	}
	//====================
	// Components
	//====================
	public static Component GetComponentInParents(this GameObject current,Type type){
		Transform node = current.transform.parent;
		while(node != null){
			Component component = node.GetComponent(type);
			if(component != null){return component;}
			node = node.parent;
		}
		return null;
	}
	public static T GetComponentInParents<T>(this GameObject current) where T : Component{
		Transform node = current.transform.parent;
		while(node != null){
			T component = node.GetComponent<T>();
			if(component != null){return component;}
			node = node.parent;
		}
		return null;
	}
	public static T[] GetComponentsInParents<T>(this GameObject current) where T : Component{
		List<T> results = new List<T>();
		Transform node = current.transform.parent;
		while(node != null){
			T[] components = node.GetComponents<T>();
			foreach(T item in components){results.Add(item);}
			node = node.parent;
		}
		return results.ToArray();
	}
	public static void EnableComponents(this GameObject current,params Type[] types){
		current.ToggleComponents(true,false,types);
	}
	public static void DisableComponents(this GameObject current,params Type[] types){
		current.ToggleComponents(false,false,types);
	}
	public static void EnableAllComponents(this GameObject current,params Type[] types){
		current.ToggleComponents(true,true,types);
	}
	public static void DisableAllComponents(this GameObject current,params Type[] types){
		current.ToggleComponents(false,true,types);
	}
	public static void ToggleComponents(this GameObject current,bool state,bool all=true,params Type[] types){
		Type renderer = typeof(Renderer);
		Type collider = typeof(Collider);
		Type behaviour = typeof(MonoBehaviour);
		Type animation = typeof(Animation);
		foreach(Type type in types){
			var components = all ? current.GetComponentsInChildren(type,true) : current.GetComponents(type);
			foreach(var item in components){
				Type itemType = item.GetType();
				Func<Type,bool> subClass = x => itemType.IsSubclassOf(x);
				Func<Type,bool> matches = x => itemType.IsAssignableFrom(x);
				if(subClass(renderer) || matches(renderer)){((Renderer)item).enabled = state;}
				else if(subClass(behaviour) || matches(behaviour)){((MonoBehaviour)item).enabled = state;}
				else if(subClass(collider) || matches(collider)){((Collider)item).enabled = state;}
				else if(subClass(animation) || matches(animation)){((Animation)item).enabled = state;}
			}
		}
	}
	public static void ToggleAllVisible(this GameObject current,bool state){
		current.ToggleComponents(state,true,typeof(Renderer));
	}
	//====================
	// Utility
	//====================
	public static void MoveTo(this GameObject current,Vector3 location,bool useX=true,bool useY=true,bool useZ=true){
		Vector3 position = current.transform.position;
		if(useX){position.x = location.x;}
		if(useY){position.y = location.y;}
		if(useZ){position.z = location.z;}
		current.transform.position = position;
	}
	public static string GetPath(this GameObject current){	
		if(current.IsNull() || current.transform.IsNull()){return "";}
		string path = current.transform.name;
		Transform parent = current.transform.parent;
		while(!parent.IsNull()){
			path = parent.name + "/" + path;
			parent = parent.parent;
		}
		return path;
	}
	public static GameObject GetParent(this GameObject current){
		if(current.transform.parent != null){
			return current.transform.parent.gameObject;
		}
		return null;
	}
	//====================
	// Find
	//====================
	public static GameObject[] FindAll(this GameObject current,string name,bool inactive=true){
		Transform[] all = current.GetComponentsInChildren<Transform>(inactive);
		List<GameObject> matches = new List<GameObject>();
		foreach(Transform transform in all){
			if(transform.name == name){
				matches.Add(transform.gameObject);
			}
		}
		return matches.ToArray();
	}
}