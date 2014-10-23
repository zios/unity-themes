using UnityEngine;
using System;
using System.Collections.Generic;
[AddComponentMenu("Zios/Singleton/Event Detector")]
public class EventDetector : MonoBehaviour{
	public void Awake(){Events.Call("OnAwake");}
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