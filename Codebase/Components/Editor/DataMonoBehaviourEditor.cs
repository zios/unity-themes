using Zios;
using UnityEngine;
using UnityEditor;
namespace Zios.UI{
    [CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
    public class DataMonoBehaviourEditor : MonoBehaviourEditor{
	    public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
		    DataMonoBehaviour target = (DataMonoBehaviour)this.target;
			var dependents = target.dependents;
			bool targetsMissing = false;
			string message = "";
		    foreach(var dependent in dependents){
				if(dependent.exists){continue;}
				message = dependent.message;
				if(dependent.target.IsNull() && dependent.dynamicTarget.Get().IsNull()){
					targetsMissing = true;
					continue;
				}
				if(!dependent.target.IsNull() || (!dependent.dynamicTarget.IsNull() && !dependent.dynamicTarget.Get().IsNull())){
					string targetName = dependent.dynamicTarget.IsNull() ? dependent.target.name : dependent.dynamicTarget.Get().name;
					if(!dependent.scriptName.IsEmpty()){targetName = dependent.scriptName;}
					message = message.Replace("[target]",targetName);
				}
				if(!dependent.types.IsNull()){
					string names = "";
					foreach(var type in dependent.types){names += type.Name + " or ";}
					message = message.Replace("[type]",names.Trim(" or "));
				}
				GUI.enabled = !dependent.processing;
				message.DrawHelp("Warning");
				GUI.enabled = true;
				if(!dependent.processing){
					Rect area = GUILayoutUtility.GetLastRect();
					EditorGUIUtility.AddCursorRect(area,MouseCursor.Link);
					if(area.Clicked(0) && dependent.method != null){
						dependent.method();
					}
				}
		    }
			if(targetsMissing){
				message = "One or more target fields are missing.";
				message.DrawHelp("Warning");
			}
		    base.OnInspectorGUI();
	    }
    }
}