using System;
using UnityEditor;
namespace Zios.Editors{
	[CustomEditor(typeof(DefaultAsset))]
	public class DefaultAssetEditor : Editor{
		public Editor instance;
		public override void OnInspectorGUI(){
			if(!this.instance.IsNull()){
				try{this.instance.OnInspectorGUI();}
				catch{
					Selection.activeObject = null;
					Utility.DelayCall(()=>{Selection.activeObject = this.target;});
				}
				return;
			}
			FileData file = FileManager.Get(this.target);
			if(file != null){
				string prefix = file.isFolder ? "Folder" : "File";
				string format = file.isFolder ? this.target.name : file.extension.ToUpper();
				string editorName = prefix + format + "Editor";
				Type type = Type.GetType(editorName);
				if(type != null && type.IsSubclassOf(typeof(Editor))){
					this.instance = Editor.CreateEditor(this.target,type);
					this.instance.OnInspectorGUI();
				}
			}
		}
	}
}