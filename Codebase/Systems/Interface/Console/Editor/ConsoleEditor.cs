using UnityEngine;
using UnityEditor;
namespace Zios.Editors{
	using Interface;
	[CustomEditor(typeof(Console))]
	public class ConsoleEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Console";
			this.header = this.header ?? FileManager.GetAsset<Texture2D>("ConsoleIcon.png");
			base.OnInspectorGUI();
		}
		[MenuItem("Zios/Settings/Console")]
		public static void Select(){
			Selection.activeObject = FileManager.GetAsset<Console>("Console.asset",false) ?? Utility.CreateSingleton("Assets/Settings/Console");
		}
	}
}