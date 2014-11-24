using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using Action = Zios.Action;
[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
public class AttributeManager : MonoBehaviour{
	public float editorInterval = 1;
	public bool editorIncludeHidden = false;
	public bool updateOnHierarchyChange = true;
	public bool updateOnAssetChange = true;
	private float nextStep;
	private bool initialSearch;
	private bool refresh;
	private bool rebuild = true;
	private AttributeBox[] boxes = new AttributeBox[0];
	private ManagedMonoBehaviour[] states = new ManagedMonoBehaviour[0];
	private AttributeExposer[] exposers = new AttributeExposer[0];
	public void OnValidate(){this.CleanEvents();}
	public void OnDestroy(){this.CleanEvents();}
	public void OnApplicationQuit(){
		Utility.EditorUpdate(this.Start,true);
	}
	public void Update(){
		if(this.updateOnAssetChange){Utility.AssetUpdate(this.PerformRefresh);}
		if(this.updateOnHierarchyChange){Utility.HierarchyUpdate(this.PerformRebuild);}
		Utility.EditorUpdate(this.Start,true);
	}
	public void CleanEvents(){
		Utility.RemoveAssetUpdate(this.PerformRefresh);
		Utility.RemoveHierarchyUpdate(this.PerformRebuild);
		Utility.RemoveEditorUpdate(this.Start);
	}
	public void PerformRefresh(){
		this.refresh = true;
	}
	[ContextMenu("Refresh")]
	public void PerformRebuild(){
		this.rebuild = true;
		this.refresh = true;
	}
	public void BuildLists(){
		bool includeHidden = Application.isPlaying || this.editorIncludeHidden;
		this.boxes = Locate.GetSceneObjects<AttributeBox>(includeHidden);
		this.states = Locate.GetSceneObjects<ManagedMonoBehaviour>(includeHidden);
		this.exposers = Locate.GetSceneObjects<AttributeExposer>(includeHidden);
	}
	public void SceneRefresh(bool full=true){
		if(full){
			Attribute.all.Clear();
			AttributeFloat.lookup.Clear();
			AttributeVector3.lookup.Clear();
			AttributeBool.lookup.Clear();
			AttributeInt.lookup.Clear();
			AttributeString.lookup.Clear();
			AttributeGameObject.lookup.Clear();
		}
		foreach(AttributeBox box in boxes){
			if(full || !box.gameObject.activeInHierarchy || !box.enabled){box.Awake();}
		}
		foreach(ManagedMonoBehaviour managed in states){
			if(full || !managed.gameObject.activeInHierarchy || !managed.enabled){managed.Awake();}
		}
		foreach(AttributeExposer exposer in exposers){
			if(full || !exposer.gameObject.activeInHierarchy || !exposer.enabled){exposer.Awake();}
		}
		this.Setup();
	}
	public void Start(){
		if(Application.isPlaying || Time.realtimeSinceStartup > this.nextStep){
			if(this.editorInterval == -1){return;}
			this.nextStep = Time.realtimeSinceStartup + this.editorInterval;
			if(!this.initialSearch || this.refresh){
				if(!this.initialSearch || this.rebuild){
					this.BuildLists();
					this.rebuild = false;
				}
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
			if(attribute.parent.IsNull()){Attribute.all.Remove(attribute);}
		}
		foreach(var attribute in Attribute.all){attribute.SetupTable();}
		foreach(var attribute in Attribute.all){attribute.SetupData();}
		Attribute.ready = true;
	}
}