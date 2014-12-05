using Zios;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
public class DataMonoBehaviourEditor : Editor{
	public float nextStep;
	public override void OnInspectorGUI(){
		GUI.changed = false;
		if(!(target is AttributeData)){
			this.DrawDefaultInspector();
		}
		Utility.EditorCall(this.EditorUpdate);
		if(GUI.changed){Utility.SetDirty(target);}
	}
	public void EditorUpdate(){
		if(Time.realtimeSinceStartup > this.nextStep){
			this.nextStep = Time.realtimeSinceStartup + 1f;
			DataMonoBehaviour target = (DataMonoBehaviour)this.target;
			target.Awake();
			if(target is StateMonoBehaviour){
				((StateMonoBehaviour)this.target).inUse.Set(false);
			}
			if(target is AttributeData){
				target.hideFlags = HideFlags.HideInInspector;
			}
		}
	}
}