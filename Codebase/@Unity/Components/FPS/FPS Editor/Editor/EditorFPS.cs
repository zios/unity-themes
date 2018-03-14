using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Components.EditorFPS{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Time;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	using Editor = UnityEditor.Editor;
	[CustomEditor(typeof(EditorFPS))]
	public class EditorFPSEditor : Editor{
		private static EditorFPSEditor instance;
		public string text;
		private int frames = 0;
		private float nextUpdate;
		public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
			EditorUI.Reset();
			EditorFPSEditor.instance = this;
			Events.Add("On Editor Update",EditorFPSEditor.EditorUpdate);
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
			if(Time.Get() >= this.nextUpdate){
				string frameText = "<b>" + this.frames.ToString() + "</b>";
				this.nextUpdate = Time.Get() + 0.5f;
				this.text = frameText + " <i>fps</i>";
				this.frames = 0;
				this.Repaint();
			}
		}
	}
}