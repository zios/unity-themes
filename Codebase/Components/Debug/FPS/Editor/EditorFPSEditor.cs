using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors.DebugEditors{
	using Interface;
	using Events;
	[CustomEditor(typeof(EditorFPS))]
	public class EditorFPSEditor : Editor{
		private static EditorFPSEditor instance;
		public string text;
		private int frames = 0;
		private float nextUpdate;
		private GUISkin skin;
		public override void OnInspectorGUI(){
			if(!UnityEvent.current.IsUseful()){return;}
			EditorFPSEditor.instance = this;
			Event.Add("On Editor Update",EditorFPSEditor.EditorUpdate);
			var style = GUI.skin.textField.RichText(true).Alignment("MiddleCenter").FixedHeight(0).FontSize(24);
			style.normal = style.focused;
			EditorUI.SetLayoutOnce(-1,40);
			this.text.ToLabel().DrawLabel(style);
			GUILayout.Space(3);
		}
		public static void EditorUpdate(){
			if(EditorFPSEditor.instance.IsNull()){return;}
			EditorFPSEditor.instance.Step();
		}
		public void Step(){
			this.frames += 1;
			if(Time.realtimeSinceStartup >= this.nextUpdate){
				string frameText = "<b>" + this.frames.ToString() + "</b>";
				this.nextUpdate = Time.realtimeSinceStartup + 0.5f;
				this.text = frameText + " <i>fps</i>";
				this.frames = 0;
				this.Repaint();
			}
		}
	}
}