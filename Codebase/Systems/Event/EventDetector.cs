#pragma warning disable 0618
using System;
using UnityEngine;
namespace Zios.Events{
	[AddComponentMenu("")][ExecuteInEditMode]
	public class EventDetector : MonoBehaviour{
		private static bool showTime = false;
		private float loadStart;
		[NonSerialized] public static bool loading = true;
		public void Loading(){
			this.loadStart = Time.realtimeSinceStartup;
			EventDetector.loading = true;
		}
		public virtual void OnValidate(){
			this.Loading();
			Event.Call("On Validate");
		}
		public virtual void Awake(){
			Event.Add("On Asset Modifying",this.Loading);
			Event.Add("On Application Quit",this.Loading);
			Event.Add("On Enter Play",this.Loading);
			Event.Add("On Exit Play",this.Loading);
			Event.Register("On Awake");
			Event.Register("On Start");
			Event.Register("On Update");
			Event.Register("On Fixed Update");
			Event.Register("On Late Update");
			Event.Register("On Enable");
			Event.Register("On Disable");
			Event.Register("On GUI");
			Event.Register("On Destroy");
			Event.Register("On Validate");
			Event.Register("On Reset");
			Event.Register("On Player Connected");
			Event.Register("On Player Disconnected");
			Event.Register("On Level Was Loaded");
			Event.Register("On Master Server Event");
			Event.Register("On Application Quit");
			Event.Register("On Application Focus");
			Event.Register("On Application Pause");
			Event.Register("On Disconnected From Server");
			Event.Call("On Awake");
		}
		public virtual void Start(){Event.Call("On Start");}
		public virtual void Update(){
			Utility.CheckLoaded(false);
			if(!Application.isLoadingLevel && EventDetector.loading){
				Event.Call("On Level Was Loaded");
				float totalTime = Mathf.Max(Time.realtimeSinceStartup-this.loadStart,0);
				if(EventDetector.showTime){
					Debug.Log("[Scene] : Load complete -- " + (totalTime) + " seconds.");
				}
				this.loadStart = 0;
				EventDetector.loading = false;
			}
			Event.Call("On Update");
		}
		public virtual void FixedUpdate(){Event.Call("On Fixed Update");}
		public virtual void LateUpdate(){Event.Call("On Late Update");}
		public virtual void OnPlayerConnected(){Event.Call("On Player Connected");}
		public virtual void OnPlayerDisconnected(){Event.Call("On Player Disconnected");}
		//public virtual void OnLevelWasLoaded(int level){Event.Call("On Level Was Loaded",level);}
		public virtual void OnMasterServerEvent(){Event.Call("On Master Server Event");}
		public virtual void OnApplicationQuit(){Event.Call("On Application Quit");}
		public virtual void OnApplicationFocus(){Event.Call("On Application Focus");}
		public virtual void OnApplicationPause(){Event.Call("On Application Pause");}
		public virtual void OnDisconnectedFromServer(){Event.Call("On Disconnected From Server");}
		public virtual void OnGUI(){Event.Call("On GUI");}
		public virtual void OnEnable(){Event.Call("On Enable");}
		public virtual void OnDisable(){Event.Call("On Disable");}
		public virtual void OnDestroy(){Event.Call("On Destroy");}
		public virtual void Reset(){Event.Call("On Reset");}
	}
}