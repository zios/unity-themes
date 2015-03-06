using Zios;
using UnityEngine;
using UnityEditor;
namespace Zios{
    [CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
    public class DataMonoBehaviourEditor : MonoBehaviourEditor{
	    public float nextUpdateStep;
	    public bool? isPrefab;
	    public bool warningReady;
	    public override void OnInspectorGUI(){
			if(Event.current.type == EventType.ScrollWheel){return;}
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
		    if(this.isPrefab == null){
			    MonoBehaviour script = (MonoBehaviour)this.target;
			    this.isPrefab = script.IsPrefab();
		    }
		    GUI.changed = false;
		    base.OnInspectorGUI();
		    if(!isData){Utility.EditorCall(this.EditorUpdate);}
		    if(GUI.changed){Utility.SetDirty(target);}
	    }
	    public void EditorUpdate(){
		    if(AttributeManager.editorInterval == -1){return;}
		    if(!(bool)this.isPrefab && Time.realtimeSinceStartup > this.nextUpdateStep){
			    this.nextUpdateStep = Time.realtimeSinceStartup + AttributeManager.editorInterval;
			    ((DataMonoBehaviour)this.target).Awake();
			    if(target is StateMonoBehaviour){
				    ((StateMonoBehaviour)this.target).inUse.Set(false);
			    }
		    }
	    }
    }
}