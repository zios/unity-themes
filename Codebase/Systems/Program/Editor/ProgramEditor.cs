using UnityEngine;
using UnityEditor;
namespace Zios.Editors{
	[CustomEditor(typeof(Program.ProgramManager))]
	public class ProgramEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Program";
			this.header = this.header ?? FileManager.GetAsset<Texture2D>("ProgramIcon.png");
			base.OnInspectorGUI();
		}
		[MenuItem("Zios/Settings/Program")]
		public static void Select(){
			Selection.activeObject = FileManager.GetAsset<Program.ProgramManager>("ProgramManager.asset",false) ?? Utility.CreateSingleton("Assets/Settings/ProgramManager");
		}
	}
}