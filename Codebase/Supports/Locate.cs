using UnityEngine;
using System.Collections.Generic;
public static class Locate{
	public static GameObject GetScenePath(string name,bool autocreate=true){
		string[] parts = name.Split('/');
		string path = "";
		GameObject current = null;
		Transform parent = null;
		foreach(string part in parts){
			path = path + "/" + part;
			current = GameObject.Find(path);
			if(current == null){
				if(!autocreate){
					return null;
				}
				current = new GameObject();
				current.name = part;
				current.transform.parent = parent; 
			}
			parent = current.transform;
		}
		return current;
	}
	public static GameObject[] FindAll(string name){
		GameObject[] all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
		List<GameObject> matches = new List<GameObject>();
		foreach(GameObject current in all){
			if(current.name == name){
				matches.Add(current);
			}
		}
		return matches.ToArray();
	}
	public static bool HasDuplicate(string name){
		GameObject[] all = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
		List<GameObject> amount = new List<GameObject>();
		foreach(var current in all){
			if(current.IsPrefab()){continue;}
			if(current.name == name){amount.Add(current);}
			if(amount.Count > 1){return true;}
		}
		return false;
	}
	public static GameObject[] GetSceneObjects(){
		GameObject[] all = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
		List<GameObject> scene = new List<GameObject>();
		foreach(var current in all){
			if(current.IsPrefab()){continue;}
			scene.Add(current);
		}
		return scene.ToArray();
	}
	public static Type[] GetSceneObjects<Type>() where Type : Component{
		Type[] all = (Type[])Resources.FindObjectsOfTypeAll(typeof(Type));
		List<Type> scene = new List<Type>();
		foreach(var current in all){
			if(current.IsPrefab()){continue;}
			scene.Add(current);
		}
		return scene.ToArray();
	}
	public static GameObject Find(string name){
		if(!name.Contains("/")){return GameObject.Find(name);}
		GameObject[] all = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
		foreach(GameObject current in all){
			if(current.IsPrefab()){continue;}
			string path = current.GetPath();
			if(path == name || path.Trim("/") == name || path.TrimLeft("/") == name || path.TrimRight("/") == name){
				return current;
			}
		}
		return null;
	}
}