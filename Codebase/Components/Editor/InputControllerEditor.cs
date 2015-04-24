using UnityEngine;
using UnityEditor;
using System.Collections;
using Zios.Inputer;
[CustomEditor(typeof(InputController))]
public class InputControllerEditor : Editor{
	public bool foldDirections = true;
	public override void OnInspectorGUI(){
		InputController target = (InputController)this.target;
		MonoScript script = MonoScript.FromMonoBehaviour(target);
		MonoScript change = (MonoScript)EditorGUILayout.ObjectField("Script",script,typeof(MonoScript),false);
		if(change != script){
			target.gameObject.AddComponent(change.GetClass().GetType());
			DestroyImmediate(target);
		}
		EditorGUILayout.LabelField("Left/Right");
		EditorGUILayout.LabelField("Forward/Backward");
	}
}