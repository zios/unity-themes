using UnityEngine;
using UnityEditor;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors{
	using Inputs;
	using Interface;
	[CustomEditor(typeof(InputManager))]
	public class InputManagerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			EditorUI.Reset();
			var target = this.target.As<InputManager>();
			if(Application.isPlaying){
				var current =  UnityEvent.current;
				if(current.isKey || current.shift || current.alt || current.control || current.command){
					if(!target.devices.Exists(x=>x.name=="Keyboard")){
						target.devices.Add(new InputDevice("Keyboard"));
					}
				}
			}
			base.OnInspectorGUI();
		}
	}
}