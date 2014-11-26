using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
public static class GameObjectExtension{
	//====================
	// Retrieval
	//====================
	public static int GetSiblingCount(this GameObject current,bool includeInactive=false){
		GameObject parent = current.GetParent();
		if(parent.IsNull()){
			Debug.Log("GameObject : Cannot locate siblings for root objects",current);
			return 0;
		}
		return parent.GetComponentsInChildren<Transform>(includeInactive).Remove(parent.transform).Length;
	}
	public static GameObject GetPreviousSibling(this GameObject current,bool includeInactive=false){
		GameObject parent = current.GetParent();
		if(parent.IsNull()){
			Debug.Log("GameObject : Cannot locate siblings for root objects",current);
			return current;
		}
		Transform[] siblings = parent.GetComponentsInChildren<Transform>(includeInactive).Remove(parent.transform);
		if(siblings.Length == 0){return current;}
		int previousIndex = siblings.IndexOf(current.transform) - 1;
		if(previousIndex < 0){previousIndex = siblings.Length-1;}
		return siblings[previousIndex].gameObject;
	}
	public static GameObject GetNextSibling(this GameObject current,bool includeInactive=false){
		GameObject parent = current.GetParent();
		if(parent.IsNull()){
			Debug.Log("GameObject : Cannot locate siblings for root objects",current);
			return current;
		}
		Transform[] siblings = parent.GetComponentsInChildren<Transform>(includeInactive).Remove(parent.transform);
		if(siblings.Length == 0){return current;}
		int nextIndex = siblings.IndexOf(current.transform) + 1;
		if(nextIndex >= siblings.Length){nextIndex = 0;}
		return siblings[nextIndex].gameObject;
	}
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
	public static T GetComponent<T>(this GameObject current,bool includeInactive=false) where T : Component{
		T[] results = current.GetComponentsInChildren<T>(includeInactive);
		foreach(T item in results){
			if(item.transform == current.transform){
				return item;
			}
		}
		return null;
	}
	public static T[] GetComponents<T>(this GameObject current,bool includeInactive=false) where T : Component{
		List<T> results = new List<T>();
		T[] search = current.GetComponentsInChildren<T>(includeInactive);
		foreach(T item in search){
			if(item.transform == current.transform){
				results.Add(item);
			}
		}
		return results.ToArray();
	}
	public static T GetComponentInParent<T>(this GameObject current,bool includeInactive=false) where T : Component{
		T[] results = current.GetComponentsInParent<T>(includeInactive);
		if(results.Length > 0){
			return results[0];
		}
		return null;
	}
	public static T GetComponentInChildren<T>(this GameObject current,bool includeInactive=false) where T : Component{
		T[] results = current.GetComponentsInChildren<T>(includeInactive);
		if(results.Length > 0){
			return results[0];
		}
		return null;
	}
	public static GameObject[] GetByName(this GameObject current,string name,bool includeInactive=true){
		Transform[] all = current.GetComponentsInChildren<Transform>(includeInactive);
		List<GameObject> matches = new List<GameObject>();
		foreach(Transform transform in all){
			if(transform.name == name){
				matches.Add(transform.gameObject);
			}
		}
		return matches.ToArray();
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
		string path = current.transform.name;
		PrefabType type = PrefabUtility.GetPrefabType(current);
		if(current.hideFlags == HideFlags.HideInHierarchy || type == PrefabType.Prefab || type == PrefabType.ModelPrefab){
			path = "Prefab/"+path;
		 }
		if(current.IsNull() || current.transform.IsNull()){return "";}
		Transform parent = current.transform.parent;
		while(!parent.IsNull()){
			path = parent.name + "/" + path;
			parent = parent.parent;
		}
		return "/" + path + "/";
	}
	public static GameObject GetParent(this GameObject current){
		if(current.transform.parent != null){
			return current.transform.parent.gameObject;
		}
		return null;
	}
	public static bool IsPrefab(this GameObject current){
		if(current.hideFlags == HideFlags.NotEditable || current.hideFlags == HideFlags.HideAndDontSave){
			return true;
		}
		#if UNITY_EDITOR
		string assetPath = AssetDatabase.GetAssetPath(current.transform.root.gameObject);
		if(!assetPath.IsEmpty()){
			return true;
		}
		#endif
		return false;
	}
}