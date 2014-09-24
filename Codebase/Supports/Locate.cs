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
}