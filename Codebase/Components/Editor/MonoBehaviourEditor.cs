using Zios;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MonoBehaviour),true)]
public class MonoBehaviourEditor : Editor{
	public float nextStep;
	public override void OnInspectorGUI(){
		this.DrawDefaultInspector();
		if(this.target is AttributeBox){Utility.EditorCall(this.EditorUpdate<AttributeBox>);}
		if(this.target is AnimationController){Utility.EditorCall(this.EditorUpdate<AnimationController>);}
		if(this.target is ColliderController){Utility.EditorCall(this.EditorUpdate<ColliderController>);}
		if(this.target is StateMonoBehaviour){Utility.EditorCall(this.EditorUpdate<StateMonoBehaviour>);}
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
		}
	}
}