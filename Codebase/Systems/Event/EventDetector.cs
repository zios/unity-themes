using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
[AddComponentMenu("Zios/Singleton/Event Detector")][ExecuteInEditMode]
public class EventDetector : MonoBehaviour{
	public void Awake(){
		Events.Register("On Start");
		Events.Register("On Update");
		Events.Register("On Player Connected");
		Events.Register("On Player Disconnected");
		Events.Register("On Level Was Loaded");
		Events.Register("On Master Server Event");
		Events.Register("On Application Quit");
		Events.Register("On Application Pause");
		Events.Register("On Disconnected From Server");
	}
	public void Start(){Events.Call("On Start");}
	public void Update(){Events.Call("On Update");}
	public void OnPlayerConnected(){Events.Call("On Player Connected");}
	public void OnPlayerDisconnected(){Events.Call("On Player Disconnected");}
	public void OnLevelWasLoaded(){Events.Call("On Level Was Loaded");}
	public void OnMasterServerEvent(){Events.Call("On Master Server Event");}
	public void OnApplicationQuit(){Events.Call("On Application Quit");}
	public void OnApplicationPause(){Events.Call("On Application Pause");}
	public void OnDisconnectedFromServer(){Events.Call("On Disconnected From Server");}
}