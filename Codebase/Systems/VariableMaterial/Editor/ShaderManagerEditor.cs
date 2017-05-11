using UnityEngine;
using UnityEditor;
namespace Zios.Editors{
	using Shaders;
	[CustomEditor(typeof(ShaderManager))]
	public class ShaderManagerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Shader";
			this.header = this.header ?? FileManager.GetAsset<Texture2D>("ShaderIcon.png");
			base.OnInspectorGUI();
			var target = this.target.As<ShaderManager>();
			if(this.changed){target.Setup();}
		}
		[MenuItem("Zios/Settings/Shader")]
		public static void Select(){
			Selection.activeObject = FileManager.GetAsset<ShaderManager>("ShaderManager.asset",false) ?? Utility.CreateSingleton("Assets/Settings/ShaderManager");
		}
	}
}