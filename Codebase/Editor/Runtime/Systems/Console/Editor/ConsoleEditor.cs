using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Console{
	using Zios.Console;
	using Zios.File;
	using Zios.Unity.Editor.MonoBehaviourEditor;
	//asm Zios.Unity.Editor.Inspectors;
	//asm Zios.Unity.Supports.Singleton;
	[CustomEditor(typeof(Console))]
	public class ConsoleEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Console";
			this.header = this.header ?? File.GetAsset<Texture2D>("ConsoleIcon.png");
			base.OnInspectorGUI();
		}
		[MenuItem("Zios/Settings/Console")]
		public static void Select(){
			Selection.activeObject = Console.Get();
		}
	}
}