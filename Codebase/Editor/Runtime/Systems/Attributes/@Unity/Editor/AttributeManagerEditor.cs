using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Attributes{
	using Zios.Attributes;
	using Zios.File;
	using Zios.Unity.Editor.MonoBehaviourEditor;
	using Zios.Unity.Log;
	//asm Zios.Unity.Editor.Inspectors;
	//asm Zios.Unity.Supports.Singleton;
	[CustomEditor(typeof(AttributeManager))]
	public class AttributeManagerEditor : MonoBehaviourEditor{
		public override void OnInspectorGUI(){
			this.title = "Attributes";
			this.header = this.header ?? File.GetAsset<Texture2D>("AttributeManagerIcon.png");
			base.OnInspectorGUI();
		}
		[MenuItem("Zios/Attribute/Full Refresh %&R")]
		public static void FullRefresh(){
			Log.Show("[AttributeManager] Manual Refresh.");
			AttributeManager.Refresh();
		}
		[MenuItem("Zios/Settings/Attributes")]
		public static void Select(){
			Selection.activeObject = AttributeManager.Get();
		}
	}
}