using Zios;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
public class DataMonoBehaviourEditor : Editor{
	public float nextStep;
	public override void OnInspectorGUI(){
		if(!(target is AttributeData)){
			this.DrawDefaultInspector();
		}
		Utility.EditorCall(this.EditorUpdate<DataMonoBehaviour>);
	}
	public void EditorUpdate<Type>() where Type : MonoBehaviour{
		if(Time.realtimeSinceStartup > this.nextStep){
			this.nextStep = Time.realtimeSinceStartup + 1f;
			Type target = (Type)this.target;
			if(target.HasMethod("Awake")){
				target.GetMethod("Awake").Invoke(target,null);
			}
			if(target is StateMonoBehaviour){
				((StateMonoBehaviour)this.target).inUse.Set(false);
			}
			if(target is AttributeData){
				target.hideFlags = HideFlags.HideInInspector;
			}
			EditorUtility.SetDirty((MonoBehaviour)this.target);
		}
	}
}