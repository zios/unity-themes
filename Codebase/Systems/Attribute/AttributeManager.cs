using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using Action = Zios.Action;
[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
public class AttributeManager : MonoBehaviour{
	public float updateEditorInterval = 1;
	public bool updateEditorOnHierarchyChange = true;
	public bool updateEditorOnAssetChange = true;
	private float nextStep;
	private bool initialSearch;
	private bool refresh;
	public void OnValidate(){this.CleanEvents();}
	public void OnDestroy(){this.CleanEvents();}
	public void OnApplicationQuit(){
		Utility.EditorUpdate(this.Start,true);
	}
	public void Update(){
		if(this.updateEditorOnAssetChange){Utility.AssetUpdate(this.ReSearch);}
		if(this.updateEditorOnHierarchyChange){Utility.HierarchyUpdate(this.ReSearch);}
		Utility.EditorUpdate(this.Start,true);
	}
	public void CleanEvents(){
		Utility.RemoveAssetUpdate(this.ReSearch);
		Utility.RemoveHierarchyUpdate(this.ReSearch);
		Utility.RemoveEditorUpdate(this.Start);
	}
	public void ReSearch(){this.refresh=true;}
	public void SceneRefresh(bool full=true){
		if(full){
			Attribute.all.Clear();
			AttributeFloat.lookup.Clear();
			AttributeVector3.lookup.Clear();
			AttributeBool.lookup.Clear();
			AttributeInt.lookup.Clear();
			AttributeString.lookup.Clear();
		}
		AttributeBox[] boxes = Locate.GetSceneObjects<AttributeBox>();
		StateMonoBehaviour[] states = Locate.GetSceneObjects<StateMonoBehaviour>();
		AttributeExposer[] exposers = Locate.GetSceneObjects<AttributeExposer>();
		foreach(AttributeBox box in boxes){
			if(full || !box.gameObject.activeInHierarchy || !box.enabled){box.Awake();}
		}
		foreach(StateMonoBehaviour state in states){
			if(full || !state.gameObject.activeInHierarchy || !state.enabled){state.Awake();}
		}
		foreach(AttributeExposer exposer in exposers){
			if(full || !exposer.gameObject.activeInHierarchy || !exposer.enabled){exposer.Awake();}
		}
		this.Setup();
	}
	public void Start(){
		if(Application.isPlaying || Time.realtimeSinceStartup > this.nextStep){
			if(this.updateEditorInterval == -1){return;}
			this.nextStep = Time.realtimeSinceStartup + this.updateEditorInterval;
			if(!this.initialSearch || this.refresh){
				this.SceneRefresh(!Application.isPlaying);
				this.initialSearch = true;
				this.refresh = false;
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