using Zios;
using UnityEngine;
using UnityEditor;
namespace Zios{
    [CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
    public class DataMonoBehaviourEditor : MonoBehaviourEditor{
	    public bool warningReady;
	    public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
		    if(Event.current.type == EventType.Layout){this.warningReady = false;}
		    DataMonoBehaviour target = (DataMonoBehaviour)this.target;
			string missing = "Attribute Manager does not exist in scene.  Attributes may not function correctly.";
			if(AttributeManager.instance == null){
				missing.DrawHelp("Warning");
				Attribute.ready = true;
			}
			bool dirty = false;
		    foreach(var item in target.warnings){
			    if(Event.current.type == EventType.Layout){this.warningReady = true;}
			    if(this.warningReady){
				    string message = item.Key;
				    var method = item.Value;
				    message.DrawHelp("Warning");
				    if(GUILayoutUtility.GetLastRect().Clicked(0)){
					    method();
						dirty = true;
				    }
			    }
		    }
			if(dirty){
				target.Awake();
				Utility.SetDirty(target);
			}
		    base.OnInspectorGUI();
	    }
    }
}