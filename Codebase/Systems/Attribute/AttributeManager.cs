using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using Action = Zios.Action;
[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
public class AttributeManager : MonoBehaviour{
	public float editorInterval = 1;
	public bool editorIncludeDisabled = true;
	public bool updateOnHierarchyChange = true;
	public bool updateOnAssetChange = true;
	private float nextStep;
	private bool setup;
	private bool refresh;
	public void OnValidate(){this.CleanEvents();}
	public void OnDestroy(){this.CleanEvents();}
	public void OnApplicationQuit(){
		Utility.EditorUpdate(this.Start,true);
	}
	public void Update(){
		if(this.updateOnAssetChange){Utility.AssetUpdate(this.PerformRefresh);}
		if(this.updateOnHierarchyChange){Utility.HierarchyUpdate(this.PerformRefresh);}
		Utility.EditorUpdate(this.Start,true);
	}
	public void CleanEvents(){
		Utility.RemoveAssetUpdate(this.PerformRefresh);
		Utility.RemoveHierarchyUpdate(this.PerformRefresh);
		Utility.RemoveEditorUpdate(this.Start);
	}
	[ContextMenu("Refresh")]
	public void PerformRefresh(){
		this.refresh = true;
	}
	public void SceneRefresh(){
		bool fullSweep = !Application.isPlaying;
		if(fullSweep){
			Attribute.all.Clear();
			Attribute.lookup.Clear();
		}
		bool includeEnabled = this.setup;
		bool includeDisabled = !this.setup || this.editorIncludeDisabled;
		DataMonoBehaviour[] data = Locate.GetSceneObjects<DataMonoBehaviour>(includeEnabled,includeDisabled);
		foreach(DataMonoBehaviour entry in data){entry.Awake();}
		this.Setup();
	}
	public void Start(){
		if(Application.isPlaying || Time.realtimeSinceStartup > this.nextStep){
			if(this.editorInterval == -1){return;}
			this.nextStep = Time.realtimeSinceStartup + this.editorInterval;
			if(!this.setup || this.refresh){
				this.SceneRefresh();
				this.setup = true;
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