using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using Action = Zios.Action;
[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
public class AttributeManager : MonoBehaviour{
	private float nextStep;
	private bool initialSearch;
	public void OnApplicationQuit(){
		Utility.EditorUpdate(this.Start,true);
	}
	public void OnDestroy(){
		Utility.RemoveAssetUpdate(this.FullRefresh);
		Utility.RemoveHierarchyUpdate(this.SceneRefresh);
		Utility.RemoveEditorUpdate(this.Start);
	}
	public void Update(){
		Utility.AssetUpdate(this.FullRefresh);
		Utility.HierarchyUpdate(this.SceneRefresh);
		Utility.EditorUpdate(this.Start,true);
	}
	public void FullRefresh(){
		this.SceneRefresh(true);
	}
	public void SceneRefresh(bool full=false){
		AttributeBox[] boxes = Locate.GetSceneObjects<AttributeBox>();
		ActionPart[] parts = Locate.GetSceneObjects<ActionPart>();
		Action[] actions = Locate.GetSceneObjects<Action>();
		foreach(AttributeBox box in boxes){
			if(full || !box.gameObject.activeInHierarchy || !box.enabled){box.Awake();}
		}
		foreach(ActionPart part in parts){
			if(full || !part.gameObject.activeInHierarchy || !part.enabled){part.Awake();}
		}
		foreach(Action action in actions){
			if(full || !action.gameObject.activeInHierarchy || !action.enabled){action.Awake();}
		}
		this.Setup();
	}
	public void Start(){
		if(Application.isPlaying || Time.realtimeSinceStartup > this.nextStep){
			this.nextStep = Time.realtimeSinceStartup + 1;
			if(!this.initialSearch){
				this.SceneRefresh(!Application.isPlaying);
				this.initialSearch = true;
				return;
			}
			this.Setup();
		}
	}
	public void Setup(){
		foreach(var attribute in Attribute.all.Copy()){
			if(attribute.parent.IsNull()){
				Attribute.all.Remove(attribute);
				continue;
			}
		}
		foreach(var attribute in Attribute.all){attribute.SetupTable();}
		foreach(var attribute in Attribute.all){attribute.SetupData();}
		Attribute.ready = true;
	}
}