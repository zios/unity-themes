using UnityEngine;
using UnityEditor;
namespace Zios.Editors{
	using Attributes;
	[CustomEditor(typeof(AttributeManager))]
	public class AttributeManagerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Attributes";
			this.header = this.header ?? FileManager.GetAsset<Texture2D>("AttributeManagerIcon.png");
			base.OnInspectorGUI();
		}
		[MenuItem("Zios/Settings/Attributes")]
		public static void Select(){
			Selection.activeObject = FileManager.GetAsset<AttributeManager>("AttributeManager.asset",false) ?? Utility.CreateSingleton("Assets/Settings/AttributeManager");
		}
	}
}