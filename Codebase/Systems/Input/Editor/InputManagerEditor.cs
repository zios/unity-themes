using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Input{
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Inputs;
	using Zios.Unity.Editor.MonoBehaviourEditor;
	using Zios.Unity.ProxyEditor;
	//asm Zios.Unity.Editor.Inspectors;
	//asm Zios.Unity.Supports.Singleton;
	[CustomEditor(typeof(InputManager))]
	public class InputManagerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Input";
			this.header = this.header ?? File.GetAsset<Texture2D>("InputIcon.png");
			base.OnInspectorGUI();
			var target = this.target.As<InputManager>();
			if(ProxyEditor.IsPlaying()){
				var current =  Event.current;
				if(current.isKey || current.shift || current.alt || current.control || current.command){
					if(!target.devices.Exists(x=>x.name=="Keyboard")){
						target.devices.Add(new InputDevice("Keyboard"));
					}
				}
			}
		}
		[MenuItem("Zios/Settings/Input")]
		public static void Select(){
			Selection.activeObject = InputManager.Get();
		}
	}
}