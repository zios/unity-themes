using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Unity.Locate{
	using Zios.Extensions;
	using Zios.Supports.Hierarchy;
	using Zios.Unity.Extensions;
	using Zios.Unity.Proxy;
	public static class Locate{
		public static bool setup;
		public static bool cleanGameObjects = false;
		public static List<Type> cleanSceneComponents = new List<Type>();
		public static List<GameObject> cleanSiblings = new List<GameObject>();
		public static Dictionary<string,GameObject> searchCache = new Dictionary<string,GameObject>();
		public static Dictionary<Type,UnityObject[]> assets = new Dictionary<Type,UnityObject[]>();
		public static Dictionary<GameObject,GameObject[]> siblings = new Dictionary<GameObject,GameObject[]>();
		public static Dictionary<GameObject,GameObject[]> enabledSiblings = new Dictionary<GameObject,GameObject[]>();
		public static Dictionary<GameObject,GameObject[]> disabledSiblings = new Dictionary<GameObject,GameObject[]>();
		public static GameObject[] rootObjects = new GameObject[0];
		public static GameObject[] sceneObjects = new GameObject[0];
		public static GameObject[] enabledObjects = new GameObject[0];
		public static GameObject[] disabledObjects = new GameObject[0];
		public static Dictionary<Type,Component[]> sceneComponents = new Dictionary<Type,Component[]>();
		public static Dictionary<Type,Component[]> enabledComponents = new Dictionary<Type,Component[]>();
		public static Dictionary<Type,Component[]> disabledComponents = new Dictionary<Type,Component[]>();
		public static Hierarchy<GameObject,Type,Component[]> objectComponents = new Hierarchy<GameObject,Type,Component[]>();
		static Locate(){
			if(!Locate.setup){
				Locate.SetDirty();
			}
		}
		public static void SetDirty(){
			Locate.cleanGameObjects = false;
			Locate.cleanSceneComponents.Clear();
			Locate.cleanSiblings.Clear();
			Locate.objectComponents.Clear();
			Locate.searchCache.Clear();
			Locate.setup = true;
		}
		public static void SetComponentsDirty<Type>() where Type : Component{Locate.cleanSceneComponents.Remove(typeof(Type));}
		public static void SetComponentsDirty<Type>(GameObject target) where Type : Component{Locate.objectComponents[target].Remove(typeof(Type));}
		public static void Build<Type>() where Type : Component{
			List<GameObject> rootObjects = new List<GameObject>();
			List<Type> enabled = new List<Type>();
			List<Type> disabled = new List<Type>();
			Type[] all = (Type[])Resources.FindObjectsOfTypeAll(typeof(Type));
			foreach(Type current in all){
				if(current.IsNull()){continue;}
				if(current.InPrefabFile()){continue;}
				if(current.gameObject.IsNull()){continue;}
				if(current.gameObject.transform.parent == null){rootObjects.Add(current.gameObject);}
				if(current.gameObject.activeInHierarchy){enabled.Add(current);}
				else{disabled.Add(current);}
			}
			Locate.sceneComponents[typeof(Type)] = enabled.Extend(disabled).ToArray();
			Locate.enabledComponents[typeof(Type)] = enabled.ToArray();
			Locate.disabledComponents[typeof(Type)] = disabled.ToArray();
			Locate.cleanSceneComponents.Add(typeof(Type));
			if(typeof(Type) == typeof(Transform)){
				List<GameObject> enabledObjects = enabled.Select(x=>x.gameObject).ToList();
				List<GameObject> disabledObjects = disabled.Select(x=>x.gameObject).ToList();
				Locate.sceneObjects = enabledObjects.Extend(disabledObjects).ToArray();
				Locate.enabledObjects = enabledObjects.ToArray();
				Locate.disabledObjects = disabledObjects.ToArray();
				Locate.rootObjects = rootObjects.ToArray();
				Locate.cleanGameObjects = true;
			}
		}
		//=====================
		// Gameobject
		//=====================
		public static bool HasDuplicate(GameObject target){
			if(Proxy.IsLoading()){return false;}
			GameObject[] siblings = target.GetSiblings(true,true,false);
			foreach(GameObject current in siblings){
				if(current.IsNull()){continue;}
				if(current.name == target.name){return true;}
			}
			return false;
		}
		public static GameObject[] GetSiblings(this GameObject current,bool includeEnabled=true,bool includeDisabled=true,bool includeSelf=true){
			if(Proxy.IsLoading()){return new GameObject[0];}
			if(!Locate.cleanSiblings.Contains(current)){
				GameObject parent = current.GetParent();
				List<GameObject> siblings;
				if(parent.IsNull()){
					Locate.GetSceneObjects(includeEnabled,includeDisabled);
					siblings = Locate.rootObjects.Remove(current).ToList();
				}
				else{
					siblings = parent.GetComponentsInChildren<Transform>(true).Select(x=>x.gameObject).ToList();
					siblings.RemoveAll(x=>x.GetParent()!=parent);
				}
				Locate.siblings[current] = siblings.ToArray();
				Locate.enabledSiblings[current] = Locate.siblings[current].Where(x=>!x.IsNull()&&x.gameObject.activeInHierarchy).Select(x=>x.gameObject).ToArray();
				Locate.disabledSiblings[current] = Locate.siblings[current].Where(x=>!x.IsNull()&&!x.gameObject.activeInHierarchy).Select(x=>x.gameObject).ToArray();
				Locate.cleanSiblings.Add(current);
			}
			GameObject[] results = Locate.enabledSiblings[current];
			if(includeEnabled && includeDisabled){results = Locate.siblings[current];}
			if(!includeEnabled){results = Locate.disabledSiblings[current];}
			if(!includeSelf){results = results.Remove(current);}
			return results;
		}
		public static GameObject[] GetByName(string name){
			if(Proxy.IsLoading()){return new GameObject[0];}
			if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
			List<GameObject> matches = new List<GameObject>();
			foreach(GameObject current in Locate.enabledObjects){
				if(current.IsNull()){continue;}
				if(current.name == name){
					matches.Add(current);
				}
			}
			return matches.ToArray();
		}
		public static GameObject[] GetSceneObjects(bool includeEnabled=true,bool includeDisabled=true){
			if(Proxy.IsLoading()){return new GameObject[0];}
			if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
			if(includeEnabled && includeDisabled){return Locate.sceneObjects;}
			if(!includeEnabled){return Locate.disabledObjects;}
			return Locate.enabledObjects;
		}
		public static GameObject GetScenePath(string name,bool autocreate=true){
			string[] parts = name.Replace("\\","/").Split('/');
			string path = "";
			GameObject current = null;
			Transform parent = null;
			foreach(string part in parts){
				path = path.IsEmpty() ? part : path + "/" + part;
				current = GameObject.Find(path);
				if(current.IsNull()){
					if(!autocreate){
						return null;
					}
					current = new GameObject(part);
					current.transform.parent = parent;
				}
				parent = current.transform;
			}
			return current;
		}
		public static GameObject Find(string name,bool includeHidden=true){
			if(Proxy.IsLoading()){return null;}
			if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
			name = name.Trim("/");
			if(Locate.searchCache.ContainsKey(name)){return Locate.searchCache[name];}
			GameObject[] all = includeHidden ? Locate.sceneObjects : Locate.enabledObjects;
			foreach(GameObject current in all){
				if(current.IsNull()){continue;}
				string path = current.GetPath().Trim("/");
				if(path == name){
					Locate.searchCache[name] = current;
					return current;
				}
			}
			return null;
		}
		//=====================
		// Components
		//=====================
		public static Type[] GetSceneComponents<Type>(bool includeEnabled=true,bool includeDisabled=true) where Type : Component{
			if(Proxy.IsLoading()){return new Type[0];}
			if(!Locate.cleanSceneComponents.Contains(typeof(Type))){Locate.Build<Type>();}
			if(includeEnabled && includeDisabled){return (Type[])Locate.sceneComponents[typeof(Type)];}
			if(!includeEnabled){return (Type[])Locate.disabledComponents[typeof(Type)];}
			return (Type[])Locate.enabledComponents[typeof(Type)];
		}
		public static Type[] GetObjectComponents<Type>(GameObject target) where Type : Component{
			if(Proxy.IsLoading()){return new Type[0];}
			if(!Locate.objectComponents.ContainsKey(target) || !Locate.objectComponents[target].ContainsKey(typeof(Type))){
				Locate.objectComponents.AddNew(target);
				Locate.objectComponents[target][typeof(Type)] = target.GetComponents<Type>(true);
			}
			return (Type[])Locate.objectComponents[target][typeof(Type)];
		}
		//=====================
		// Assets
		//=====================
		public static object[] GetAssets(Type type){
			if(Proxy.IsLoading()){return new object[0];}
			if(!Locate.assets.ContainsKey(type)){Locate.assets[type] = Resources.FindObjectsOfTypeAll(type);}
			return Locate.assets[type];
		}
		public static Type[] GetAssets<Type>() where Type : UnityObject{
			if(Proxy.IsLoading()){return new Type[0];}
			if(!Locate.assets.ContainsKey(typeof(Type))){Locate.assets[typeof(Type)] = Resources.FindObjectsOfTypeAll(typeof(Type));}
			return (Type[])Locate.assets[typeof(Type)];
		}
		public static Type GetAsset<Type>(string name) where Type : UnityObject{
			if(Proxy.IsLoading()){return default(Type);}
			return Locate.GetAssets<Type>().Where(x=>x.name==name).FirstOrDefault();
		}
	}
}
namespace Zios.Unity.Locate{
	using Zios.Extensions;
	public static class GameObjectExtension{
		public static int GetSiblingCount(this GameObject current,bool includeInactive=false){
			return Locate.GetSiblings(current,true,includeInactive).Length;
		}
		public static GameObject GetPreviousSibling(this GameObject current,bool includeInactive=false){
			GameObject[] siblings = Locate.GetSiblings(current,true,includeInactive);
			if(siblings.Length == 0){return current;}
			int previousIndex = siblings.IndexOf(current) - 1;
			if(previousIndex < 0){previousIndex = siblings.Length-1;}
			return siblings[previousIndex].gameObject;
		}
		public static GameObject GetNextSibling(this GameObject current,bool includeInactive=false){
			GameObject[] siblings = Locate.GetSiblings(current,true,includeInactive);
			if(siblings.Length == 0){return current;}
			int nextIndex = siblings.IndexOf(current) + 1;
			if(nextIndex >= siblings.Length){nextIndex = 0;}
			return siblings[nextIndex].gameObject;
		}
	}
}