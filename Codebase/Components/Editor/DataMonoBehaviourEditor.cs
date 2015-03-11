using Zios;
using UnityEngine;
using UnityEditor;
namespace Zios{
    [CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
    public class DataMonoBehaviourEditor : MonoBehaviourEditor{
	    public float nextUpdateStep;
	    public bool warningReady;
	    public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
		    if(Event.current.type == EventType.Layout){this.warningReady = false;}
		    DataMonoBehaviour target = (DataMonoBehaviour)this.target;
		    bool isData = this.target is AttributeData;
		    foreach(var item in target.warnings){
			    if(Event.current.type == EventType.Layout){this.warningReady = true;}
			    if(this.warningReady){
				    string message = item.Key;
				    var method = item.Value;
				    message.DrawHelp("Warning");
				    if(GUILayoutUtility.GetLastRect().Clicked(0)){
					    method();
					    Utility.SetDirty(target);
				    }
			    }
		    }
		    if(isData && PlayerPrefs.GetInt("ShowAttributeData") == 0){return;}
		    GUI.changed = false;
		    base.OnInspectorGUI();
		    if(GUI.changed){Utility.SetDirty(target);}
	    }
    }
}