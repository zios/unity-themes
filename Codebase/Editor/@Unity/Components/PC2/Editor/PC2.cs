namespace Zios.Unity.Editor.Components.PC2{
	using Zios.File;
	using Zios.Unity.Components.PC2;
	using Zios.Unity.EditorUI;
	public class FilePC2Editor : UnityEditor.Editor{
		public PC2Data data;
		public override void OnInspectorGUI(){
			EditorUI.Reset();
			//GUI.enabled = true;
			if(this.data == null){
				FileData file = File.Get(this.target);
				this.data = new PC2Data();
				data.Load(file.path);
			}
			this.data.DrawFields("");
		}
	}
}