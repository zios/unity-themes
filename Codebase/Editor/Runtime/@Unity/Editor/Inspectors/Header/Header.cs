using UnityEngine;
namespace Zios.Unity.Editor.Inspectors{
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Extensions;
	using Editor = UnityEditor.Editor;
	public class HeaderEditor : Editor{
		public string title = "Inspector";
		public Texture2D header;
		public override GUIContent GetPreviewTitle(){return new GUIContent("");}
		public override bool HasPreviewGUI(){return true;}
		public override void DrawPreview(Rect previewArea){}
		public override void OnPreviewGUI(Rect area,GUIStyle background){
			this.target.name = this.title;
			base.OnPreviewGUI(area,background.Background(this.header));
		}
		public override void OnInspectorGUI(){
			GUI.changed = false;
			this.serializedObject.Update();
			ProxyEditor.GetInspector(this).SetTitle(this.title);
			base.OnInspectorGUI();
			if(GUI.changed){
				ProxyEditor.SetDirty(this.target);
				this.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}