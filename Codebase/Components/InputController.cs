using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
namespace Zios.Inputer{
	[AddComponentMenu("Zios/Component/General/InputController")]
	public class InputController : MonoBehaviour{
		public List<Handler> handlers;
		public void Update(){
			foreach(Handler handler in handlers){
				if(handler.Check()){
					handler.Process();
				}
			}
		}
	}
	public class Handler{
		public bool enabled;
		public bool active;
		public Accessor targetVariable;
		public string keyName;
		public float triggerDelay;
		public float triggerDuration;
		public KeyState trigger;
		public virtual bool Check(){
			if(enabled){
				return true;
			}
			return false;
		}
		public virtual void Process(){}
	}
	public class Unity : Handler{
		public string inputName;
	}
	public class Keyboard : Handler{
		public KeyCode key;
	}
	public class Mouse : Handler{
		public KeyCode button;
	}
	public class Gamepad : Handler{
		public int id;
	}
	public enum KeyState{Down,Up,Released,Pressed,DoubleTap}
}
