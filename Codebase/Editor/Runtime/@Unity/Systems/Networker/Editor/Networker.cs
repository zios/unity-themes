using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Networker{
	using Zios.File;
	using Zios.Unity.Editor.MonoBehaviourEditor;
	using Zios.Unity.Networker;
	//asm Zios.Unity.Editor.Inspectors;
	//asm Zios.Unity.Supports.Singleton;
	[CustomEditor(typeof(Networker))]
	public class NetworkerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Networker";
			this.header = this.header ?? File.GetAsset<Texture2D>("NetworkerIcon.png");
			base.OnInspectorGUI();
		}
		[MenuItem("Zios/Settings/Networker")]
		public static void Select(){
			Selection.activeObject = Networker.Get();
		}
	}
}