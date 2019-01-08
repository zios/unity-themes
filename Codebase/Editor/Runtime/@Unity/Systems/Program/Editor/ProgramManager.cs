using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.ProgramManager{
	using Zios.File;
	using Zios.Unity.Editor.MonoBehaviourEditor;
	using Zios.Unity.ProgramManager;
	//asm Zios.Unity.Editor.Inspectors;
	//asm Zios.Unity.Supports.Singleton;
	[CustomEditor(typeof(ProgramManager))]
	public class ProgramManagerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Program";
			this.header = this.header ?? File.GetAsset<Texture2D>("ProgramIcon.png");
			base.OnInspectorGUI();
		}
		[MenuItem("Zios/Settings/Program")]
		public static void Select(){
			Selection.activeObject = ProgramManager.Get();
		}
	}
}