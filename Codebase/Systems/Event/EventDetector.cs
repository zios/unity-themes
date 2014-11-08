using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
[AddComponentMenu("Zios/Singleton/Event Detector")][ExecuteInEditMode]
public class EventDetector : MonoBehaviour{
	#if UNITY_EDITOR
	public void EnterPlay(){
		string path = Application.dataPath+"/../Playing.lock";
		if(EditorApplication.isPlaying){
			FileManager.WriteFile(path,new byte[0]);
		}
		else{
			FileManager.DeleteFile(path);
		}
	}
	#endif
	public void Awake(){
		#if UNITY_EDITOR
		EditorApplication.playmodeStateChanged += this.EnterPlay;
		#endif
		Events.Call("OnAwake");
	}
	public void Start(){Events.Call("OnStart");}
	public void Update(){Events.Call("OnUpdate");}
	public void OnPlayerConnected(){Events.Call("OnPlayerConnected");}
	public void OnPlayerDisconnected(){Events.Call("OnPlayerDisconnected");}
	public void OnLevelWasLoaded(){Events.Call("OnLevelWasLoaded");}
	public void OnMasterServerEvent(){Events.Call("OnMasterServerEvent");}
	public void OnApplicationQuit(){Events.Call("OnApplicationQuit");}
	public void OnApplicationPause(){Events.Call("OnApplicationPause");}
	public void OnDisconnectedFromServer(){Events.Call("OnDisconnectedFromServer");}
}