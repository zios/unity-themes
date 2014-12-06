using Zios;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
public class DataMonoBehaviourEditor : Editor{
	public float nextStep;
	public override void OnInspectorGUI(){
		GUI.changed = false;
		this.DrawDefaultInspector();
		Utility.EditorCall(this.EditorUpdate);
		if(GUI.changed){Utility.SetDirty(target);}
	}
	public void EditorUpdate(){
		MonoBehaviour script = (MonoBehaviour)this.target;
		if(!script.IsPrefab() && Time.realtimeSinceStartup > this.nextStep){
			this.nextStep = Time.realtimeSinceStartup + 1f;
			((DataMonoBehaviour)this.target).Awake();
			if(target is StateMonoBehaviour){
				((StateMonoBehaviour)this.target).inUse.Set(false);
			}
		}
	}
}