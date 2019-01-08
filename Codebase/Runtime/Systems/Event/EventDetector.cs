using UnityEngine;
namespace Zios.Events{
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	using Zios.Unity.Time;
	[AddComponentMenu("")][ExecuteInEditMode]
	public class EventDetector : MonoBehaviour{
		private static bool showTime = false;
		private float loadStart;
		public static bool loading = true;
		public void Loading(){
			Proxy.busyMethods.Add(()=>EventDetector.loading);
			this.loadStart = Time.Get();
			EventDetector.loading = true;
		}
		public virtual void OnValidate(){
			this.Loading();
			Events.Call("On Validate");
		}
		public virtual void Awake(){
			Events.Add("On Asset Modifying",this.Loading);
			Events.Add("On Application Quit",this.Loading);
			Events.Add("On Enter Play",this.Loading);
			Events.Add("On Exit Play",this.Loading);
			Events.Register("On Awake");
			Events.Register("On Start");
			Events.Register("On Update");
			Events.Register("On Fixed Update");
			Events.Register("On Late Update");
			Events.Register("On Enable");
			Events.Register("On Disable");
			Events.Register("On GUI");
			Events.Register("On GUI Repaint");
			Events.Register("On GUI Layout");
			Events.Register("On GUI Key Down");
			Events.Register("On GUI Key Up");
			Events.Register("On GUI Scroll Wheel");
			Events.Register("On GUI Context Click");
			Events.Register("On GUI Mouse Down");
			Events.Register("On GUI Mouse Up");
			Events.Register("On GUI Mouse Move");
			Events.Register("On GUI Mouse Drag");
			Events.Register("On GUI Mouse Enter Window");
			Events.Register("On GUI Mouse Leave Window");
			Events.Register("On GUI Drag Perform");
			Events.Register("On GUI Drag Exited");
			Events.Register("On GUI Drag Updated");
			Events.Register("On GUI Validate Command");
			Events.Register("On GUI Ignore");
			Events.Register("On GUI Used");
			Events.Register("On Destroy");
			Events.Register("On Validate");
			Events.Register("On Reset");
			Events.Register("On Player Connected");
			Events.Register("On Player Disconnected");
			Events.Register("On Level Was Loaded");
			Events.Register("On Master Server Event");
			Events.Register("On Application Quit");
			Events.Register("On Application Focus");
			Events.Register("On Application Pause");
			Events.Register("On Disconnected From Server");
			Events.Call("On Awake");
		}
		public virtual void Start(){Events.Call("On Start");}
		public virtual void Update(){
			//Utility.CheckLoaded(false);
			if(!Proxy.IsLoading() && EventDetector.loading){
				Events.Call("On Level Was Loaded");
				float totalTime = Mathf.Max(Time.Get()-this.loadStart,0);
				if(EventDetector.showTime){
					Log.Show("[Scene] : Load complete -- " + (totalTime) + " seconds.");
				}
				this.loadStart = 0;
				EventDetector.loading = false;
			}
			Events.Call("On Update");
		}
		public virtual void FixedUpdate(){Events.Call("On Fixed Update");}
		public virtual void LateUpdate(){Events.Call("On Late Update");}
		public virtual void OnPlayerConnected(){Events.Call("On Player Connected");}
		public virtual void OnPlayerDisconnected(){Events.Call("On Player Disconnected");}
		//public virtual void OnLevelWasLoaded(int level){Event.Call("On Level Was Loaded",level);}
		public virtual void OnMasterServerEvent(){Events.Call("On Master Server Event");}
		public virtual void OnApplicationQuit(){Events.Call("On Application Quit");}
		public virtual void OnApplicationFocus(){Events.Call("On Application Focus");}
		public virtual void OnApplicationPause(){Events.Call("On Application Pause");}
		public virtual void OnDisconnectedFromServer(){Events.Call("On Disconnected From Server");}
		public virtual void OnGUI(){
			Events.Call("On GUI");
			var current = Event.current.type;
			if(current == EventType.Repaint){Events.Call("On GUI Repaint");}
			else if(current == EventType.Layout){Events.Call("On GUI Layout");}
			else if(current == EventType.KeyDown){Events.Call("On GUI Key Down");}
			else if(current == EventType.KeyUp){Events.Call("On GUI Key Up");}
			else if(current == EventType.ScrollWheel){Events.Call("On GUI Scroll Wheel");}
			else if(current == EventType.ContextClick){Events.Call("On GUI Context Click");}
			else if(current == EventType.MouseDown){Events.Call("On GUI Mouse Down");}
			else if(current == EventType.MouseUp){Events.Call("On GUI Mouse Up");}
			else if(current == EventType.MouseMove){Events.Call("On GUI Mouse Move");}
			else if(current == EventType.MouseDrag){Events.Call("On GUI Mouse Drag");}
			else if(current == EventType.MouseEnterWindow){Events.Call("On GUI Mouse Enter Window");}
			else if(current == EventType.MouseLeaveWindow){Events.Call("On GUI Mouse Leave Window");}
			else if(current == EventType.DragPerform){Events.Call("On GUI Drag Perform");}
			else if(current == EventType.DragExited){Events.Call("On GUI Drag Exited");}
			else if(current == EventType.DragUpdated){Events.Call("On GUI Drag Updated");}
			else if(current == EventType.ValidateCommand){Events.Call("On GUI Validate Command");}
			else if(current == EventType.Ignore){Events.Call("On GUI Ignore");}
			else if(current == EventType.Used){Events.Call("On GUI Used");}
		}
		public virtual void OnEnable(){Events.Call("On Enable");}
		public virtual void OnDisable(){Events.Call("On Disable");}
		public virtual void OnDestroy(){
			Events.Call("On Destroy");
			Events.RemoveAll(this);
		}
		public virtual void Reset(){Events.Call("On Reset");}
	}
}
