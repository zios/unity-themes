using Zios;
using UnityEngine;
using UnityEditor;
namespace Zios{
    [CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
    public class DataMonoBehaviourEditor : MonoBehaviourEditor{
	    public override void OnInspectorGUI(){
			if(Utility.IsPlaying() || Application.isLoadingLevel){
				this.DrawDefaultInspector();
				return;
			}
			if(!Event.current.IsUseful()){return;}
		    DataMonoBehaviour target = (DataMonoBehaviour)this.target;
			var dependents = target.dependents;
			bool dependentsChanged = false;
		    foreach(var dependent in dependents){
				if(dependent.exists){continue;}
				string message = dependent.message;
				if(!dependent.target.IsNull() || (!dependent.dynamicTarget.IsNull() && !dependent.dynamicTarget.Get().IsNull())){
					string targetName = dependent.dynamicTarget.IsNull() ? dependent.target.name : dependent.dynamicTarget.Get().name;
					if(!dependent.scriptName.IsEmpty()){targetName = dependent.scriptName;}
					message = message.Replace("[target]",targetName);
				}
				if(!dependent.type.IsNull()){
					message = message.Replace("[type]",dependent.type.Name);
				}
				message.DrawHelp("Warning");
				Rect area = GUILayoutUtility.GetLastRect();
				EditorGUIUtility.AddCursorRect(area,MouseCursor.Link);
				if(area.Clicked(0) && dependent.method != null){
					dependent.method();
					dependentsChanged = true;
				}
		    }
			if(dependentsChanged){
				target.Awake();
			}
		    base.OnInspectorGUI();
	    }
    }
}