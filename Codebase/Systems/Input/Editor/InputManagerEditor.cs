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
			Utility.GetInspector(this).SetTitle("Input");
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
			if(Utility.IsRepainting()){
				Utility.GetInspector(this).SetTitle("Inspector");
			}
		}
		public override bool HasPreviewGUI(){return true;}
		public override void DrawPreview(Rect previewArea){}
		public override void OnPreviewGUI(Rect area,GUIStyle background){
			base.OnPreviewGUI(area,background.Background(FileManager.GetAsset<Texture2D>("InputIcon.png")));
		}
		public override GUIContent GetPreviewTitle(){return new GUIContent("");}
		[MenuItem("Zios/System/Input")]
		public static void Select(){
			Selection.activeObject = FileManager.GetAsset<InputManager>("InputManager.asset",false) ?? Utility.CreateSingleton("Assets/Settings/InputManager");
		}
	}
}