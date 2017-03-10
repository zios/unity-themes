using UnityEngine;
using UnityEditor;
namespace Zios.Editors{
	[CustomEditor(typeof(Networker))]
	public class NetworkerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Networker";
			this.header = this.header ?? FileManager.GetAsset<Texture2D>("NetworkerIcon.png");
			base.OnInspectorGUI();
		}
		[MenuItem("Zios/Settings/Networker")]
		public static void Select(){
			Selection.activeObject = FileManager.GetAsset<Networker>("Networker.asset",false) ?? Utility.CreateSingleton("Assets/Settings/Networker");
		}
	}
}