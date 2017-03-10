using UnityEngine;
using UnityEditor;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors{
	using Inputs;
	[CustomEditor(typeof(InputManager))]
	public class InputManagerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Input";
			this.header = this.header ?? FileManager.GetAsset<Texture2D>("InputIcon.png");
			base.OnInspectorGUI();
			var target = this.target.As<InputManager>();
			if(Application.isPlaying){
				var current =  UnityEvent.current;
				if(current.isKey || current.shift || current.alt || current.control || current.command){
					if(!target.devices.Exists(x=>x.name=="Keyboard")){
						target.devices.Add(new InputDevice("Keyboard"));
					}
				}
			}
		}
		[MenuItem("Zios/Settings/Input")]
		public static void Select(){
			Selection.activeObject = FileManager.GetAsset<InputManager>("InputManager.asset",false) ?? Utility.CreateSingleton("Assets/Settings/InputManager");
		}
	}
}