using Zios;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
public class DataMonoBehaviourEditor : Editor{
	public float nextStep;
	public bool? isPrefab;
	public override void OnInspectorGUI(){
		bool isData = this.target is AttributeData;
		if(isData && PlayerPrefs.GetInt("ShowAttributeData") == 0){return;}
		if(this.isPrefab == null){
			MonoBehaviour script = (MonoBehaviour)this.target;
			this.isPrefab = script.IsPrefab();
		}
		GUI.changed = false;
		this.DrawDefaultInspector();
		if(!isData){Utility.EditorCall(this.EditorUpdate);}
		if(GUI.changed){Utility.SetDirty(target);}
	}
	public void EditorUpdate(){
		if(!(bool)this.isPrefab && Time.realtimeSinceStartup > this.nextStep){
			this.nextStep = Time.realtimeSinceStartup + AttributeManager.editorInterval;
			((DataMonoBehaviour)this.target).Awake();
			if(target is StateMonoBehaviour){
				((StateMonoBehaviour)this.target).inUse.Set(false);
			}
		}
	}
}