using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Extensions{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Proxy;
	public static class GameObjectExtension{
		//====================
		// Retrieval
		//====================
		public static GameObject[] GetByName(this GameObject current,string name,bool includeInactive=true){
			if(current.IsNull()){return null;}
			Transform[] all = current.GetComponentsInChildren<Transform>(includeInactive);
			List<GameObject> matches = new List<GameObject>();
			foreach(Transform transform in all){
				if(transform.name == name){
					matches.Add(transform.gameObject);
				}
			}
			return matches.ToArray();
		}
		public static Mesh GetMesh(this GameObject current){
			if(current.IsNull()){return null;}
			return current.transform.GetMesh();
		}
		public static Mesh[] GetMeshes(this GameObject current){
			if(current.IsNull()){return new Mesh[0];}
			return current.transform.GetMeshes();
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
		public static GameObject Destroy<Type>(this GameObject current) where Type : Component{return current.RemoveComponent<Type>();}
		public static GameObject Remove<Type>(this GameObject current) where Type : Component{return current.RemoveComponent<Type>();}
		public static GameObject RemoveComponent<Type>(this GameObject current) where Type : Component{
			var target = current.GetComponent<Type>();
			if(!target.IsNull()){
				target.Destroy();
			}
			return current;
		}
		public static Type Add<Type>(this GameObject current) where Type : Component{return current.AddComponent<Type>();}
		public static Type Get<Type>(this GameObject current){
			return current.GetComponent<Type>();
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
		public static bool Has<T>(this GameObject current,bool includeInactive=false) where T : Component{return current.HasComponent<T>(includeInactive);}
		public static bool HasComponent<T>(this GameObject current,bool includeInactive=false) where T : Component{
			if(current.IsNull()){return false;}
			return !current.GetComponent<T>(includeInactive).IsNull();
		}
		public static T GetComponent<T>(this GameObject current,bool includeInactive=false) where T : Component{
			if(current.IsNull()){return null;}
			T[] results = current.GetComponentsInChildren<T>(includeInactive);
			foreach(T item in results){
				if(item.transform == current.transform){
					return item;
				}
			}
			return null;
		}
		public static T[] GetComponents<T>(this GameObject current,bool includeInactive=false) where T : Component{
			if(current.IsNull()){return null;}
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
			if(current.IsNull()){return null;}
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
		public static void Remove(this GameObject current){current.Destroy();}
		public static void Destroy(this GameObject current){
			if(Application.isPlaying){GameObject.Destroy(current);}
			else{GameObject.DestroyImmediate(current);}
		}
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
			if(Proxy.IsEditor()){
				var type = ProxyEditor.GetPrefabType(current);
				if(type.ContainsAny("Prefab","ModelPrefab")){
					path = "Prefab/"+path;
				}
			}
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
			if(current.IsNull()){return false;}
			if(Proxy.IsEditor()){
				return !ProxyEditor.GetPrefabType(current.transform.root.gameObject).IsNull();
			}
			return false;
		}
		public static bool InPrefabFile(this Component current){
			return !current.IsNull() && current.gameObject.IsPrefabFile();
		}
		public static bool IsPrefabFile(this GameObject current){
			if(current.IsNull()){return false;}
			if(current.hideFlags == HideFlags.NotEditable || current.hideFlags == HideFlags.HideAndDontSave){
				return true;
			}
			if(Proxy.IsEditor()){
				string assetPath = ProxyEditor.GetAssetPath(current.transform.root.gameObject);
				if(!assetPath.IsEmpty()){
					return true;
				}
			}
			return false;
		}
	}
}