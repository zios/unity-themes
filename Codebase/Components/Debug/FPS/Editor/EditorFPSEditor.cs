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
			string skinName = EditorGUIUtility.isProSkin || EditorPrefs.GetBool("EditorTheme-Dark",false) ? "Dark" : "Light";
			if(this.skin == null || !this.skin.name.Contains(skinName)){
				this.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
			}
			GUI.skin = this.skin;
			this.text.ToLabel().DrawLabel(GUI.skin.GetStyle("LargeLabel"));
		}
		public static void EditorUpdate(){
			if(EditorFPSEditor.instance.IsNull()){return;}
			EditorFPSEditor.instance.Step();
		}
		public void Step(){
			this.frames += 1;
			if(Time.realtimeSinceStartup >= this.nextUpdate){
				string color = EditorGUIUtility.isProSkin || EditorPrefs.GetBool("EditorTheme-Dark",false) ? "white" : "black";
				string frameText = "<color="+color+">" + this.frames.ToString() + "</color>";
				this.nextUpdate = Time.realtimeSinceStartup + 0.5f;
				this.text = frameText + " fps";
				this.frames = 0;
				this.Repaint();
			}
		}
	}
}