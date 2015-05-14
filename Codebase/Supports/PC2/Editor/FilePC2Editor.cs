using UnityEngine;
using UnityEditor;
using Zios;
public class FilePC2Editor : Editor{
	public PC2Data data;
	public override void OnInspectorGUI(){
		//GUI.enabled = true;
		if(this.data == null){
			FileData file = FileManager.Get(this.target);
			this.data = new PC2Data();
			data.Load(file.path);
		}
		this.data.DrawFields("");
	}
}