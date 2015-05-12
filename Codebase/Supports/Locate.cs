using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
namespace Zios{
    #if UNITY_EDITOR
    using UnityEditor;
    [InitializeOnLoad]
    #endif
    public static class Locate{
	    public static bool cleanGameObjects = false;
	    public static List<Type> cleanSceneComponents = new List<Type>();
	    public static List<GameObject> cleanSiblings = new List<GameObject>();
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
	    public static Dictionary<GameObject,Dictionary<Type,Component[]>> objectComponents = new Dictionary<GameObject,Dictionary<Type,Component[]>>();
	    static Locate(){
		    //Events.Add("On Application Quit",Locate.SetDirty);
			Events.Add("On Scene Loaded",Locate.SetDirty);
			Events.Add("On Hierarchy Changed",Locate.SetDirty);
		    Locate.SetDirty();
	    }
	    public static void SetDirty(){
		    Locate.cleanGameObjects = false;
		    Locate.cleanSceneComponents.Clear();
		    Locate.cleanSiblings.Clear();
		    Locate.objectComponents.Clear();
	    }
	    public static GameObject GetScenePath(string name,bool autocreate=true){
		    string[] parts = name.Split('/');
		    string path = "";
		    GameObject current = null;
		    Transform parent = null;
		    foreach(string part in parts){
			    path = path + "/" + part;
			    current = Locate.Find(path);
			    if(current.IsNull()){
				    if(!autocreate){
					    return null;
				    }
				    current = new GameObject(part);
				    current.transform.parent = parent;
					Locate.SetDirty();
			    }
			    parent = current.transform;
		    }
		    return current;
	    }
	    public static void Build<Type>() where Type : Component{
		    List<GameObject> rootObjects = new List<GameObject>();
		    List<Type> enabled = new List<Type>();
		    List<Type> disabled = new List<Type>();
		    Type[] all = (Type[])Resources.FindObjectsOfTypeAll(typeof(Type));
		    foreach(Type current in all){
			    if(current.IsNull()){continue;}
			    if(current.IsPrefab()){continue;}
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
	    public static GameObject[] GetByName(string name){
			if(Application.isLoadingLevel){return new GameObject[0];}
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
	    public static bool HasDuplicate(GameObject target){
			if(Application.isLoadingLevel){return false;}
		    GameObject[] siblings = target.GetSiblings(true,true,false);
		    foreach(GameObject current in siblings){
				if(current.IsNull()){continue;}
			    if(current.name == target.name){return true;}
		    }
		    return false;
	    }
	    public static GameObject[] GetSiblings(this GameObject current,bool includeEnabled=true,bool includeDisabled=true,bool includeSelf=true){
			if(Application.isLoadingLevel){return new GameObject[0];}
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
			    Locate.enabledSiblings[current] = Locate.siblings[current].Where(x=>x.gameObject.activeInHierarchy).Select(x=>x.gameObject).ToArray();
			    Locate.disabledSiblings[current] = Locate.siblings[current].Where(x=>!x.gameObject.activeInHierarchy).Select(x=>x.gameObject).ToArray();
			    Locate.cleanSiblings.Add(current);
		    }
		    GameObject[] results = Locate.enabledSiblings[current];
		    if(includeEnabled && includeDisabled){results = Locate.siblings[current];}
		    if(!includeEnabled){results = Locate.disabledSiblings[current];}
		    if(!includeSelf){results = results.Remove(current);}
		    return results;
	    }
	    public static GameObject[] GetSceneObjects(bool includeEnabled=true,bool includeDisabled=true){
			if(Application.isLoadingLevel){return new GameObject[0];}
		    if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
		    if(includeEnabled && includeDisabled){return Locate.sceneObjects;}
		    if(!includeEnabled){return Locate.disabledObjects;}
		    return Locate.enabledObjects;
	    }
	    public static Type[] GetSceneComponents<Type>(bool includeEnabled=true,bool includeDisabled=true) where Type : Component{
			if(Application.isLoadingLevel){return new Type[0];}
		    if(!Locate.cleanSceneComponents.Contains(typeof(Type))){Locate.Build<Type>();}
		    if(includeEnabled && includeDisabled){return (Type[])Locate.sceneComponents[typeof(Type)];}
		    if(!includeEnabled){return (Type[])Locate.disabledComponents[typeof(Type)];}
		    return (Type[])Locate.enabledComponents[typeof(Type)];
	    }
	    public static Type[] GetObjectComponents<Type>(GameObject target) where Type : Component{
			if(Application.isLoadingLevel){return new Type[0];}
		    if(!Locate.objectComponents.ContainsKey(target) || !Locate.objectComponents[target].ContainsKey(typeof(Type))){
			    Locate.objectComponents.AddNew(target);
			    Locate.objectComponents[target][typeof(Type)] = target.GetComponents<Type>(true);
		    }
		    return (Type[])Locate.objectComponents[target][typeof(Type)];
	    }
	    public static GameObject Find(string name,bool includeHidden=true){
			if(Application.isLoadingLevel){return null;}
		    if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
			name = name.Trim("/");
		    GameObject[] all = includeHidden ? Locate.sceneObjects : Locate.enabledObjects;
		    foreach(GameObject current in all){
				if(current.IsNull()){continue;}
			    string path = current.GetPath().Trim("/");
			    if(path == name){
				    return current;
			    }
		    }
		    return null;
	    }
    }
}