using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	public class HeaderEditor : Editor{
		public string title = "Inspector";
		public Texture2D header;
		public override GUIContent GetPreviewTitle(){return new GUIContent("");}
		public override bool HasPreviewGUI(){return true;}
		public override void DrawPreview(Rect previewArea){}
		public override void OnPreviewGUI(Rect area,GUIStyle background){base.OnPreviewGUI(area,background.Background(this.header));}
		public override void OnInspectorGUI(){
			GUI.changed = false;
			this.serializedObject.Update();
			Utility.GetInspector(this).SetTitle(this.title);
			base.OnInspectorGUI();
			if(GUI.changed){
				Utility.SetDirty(this.target);
				this.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}