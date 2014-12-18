using Zios;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using Action = Zios.Action;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
public class AttributeManager : MonoBehaviour{
	public static bool refresh;
	public static float editorInterval = 1;
	public float updateInterval = 1;
	public bool editorIncludeDisabled = true;
	public bool updateOnHierarchyChange = false;
	public bool updateOnAssetChange = false;
	private float nextStep;
	private bool setup;
	#if UNITY_EDITOR
    [MenuItem("Zios/Process/Attribute/Remove Visible Data")]
	static void RemoveVisibleData(){AttributeManager.RemoveAttributeData(true);}
    [MenuItem("Zios/Process/Attribute/Remove All Data")]
	static void RemoveAttributeData(bool visibleOnly=false){
		foreach(UnityObject current in Selection.objects){
			if(current is GameObject){
				GameObject gameObject = (GameObject)current;
				foreach(AttributeData data in Locate.GetObjectComponents<AttributeData>(gameObject)){
					bool canDestroy = !visibleOnly || (visibleOnly && !data.hideFlags.Contains(HideFlags.HideInInspector));
					if(canDestroy){
						Utility.Destroy(data);
					}
				}
			}
		}
	}
    [MenuItem("Zios/Process/Attribute/Hide All Data")]
	static void HideAttributeData(){
		PlayerPrefs.SetInt("ShowAttributeData",0);
		AttributeManager.refresh = true;
	}
    [MenuItem("Zios/Process/Attribute/Show All Data")]
	static void ShowAttributeData(){
		PlayerPrefs.SetInt("ShowAttributeData",1);
		AttributeManager.refresh = true;
	}
    [MenuItem("Zios/Process/Attribute/Full Refresh")]
	static void AttributeRefresh(){
		AttributeManager.refresh = true;
	}	
	#endif
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
		AttributeManager.refresh = true;
	}
	public void SceneRefresh(){
		bool fullSweep = !Application.isPlaying;
		if(fullSweep){
			Attribute.all.Clear();
			Attribute.lookup.Clear();
		}
		bool includeEnabled = this.setup || !Application.isPlaying;
		bool includeDisabled = !this.setup || this.editorIncludeDisabled;
		DataMonoBehaviour[] data = Locate.GetSceneComponents<DataMonoBehaviour>(includeEnabled,includeDisabled);
		//data = data.OrderBy(x=>x.GetType().ToString()).ToArray();
		foreach(DataMonoBehaviour entry in data){
			if(AttributeManager.refresh){return;}
			entry.Awake();
		}
		if(!Application.isPlaying){
			Debug.Log("AttributeData Count : " + data.Count(x=>x is AttributeData));
			foreach(DataMonoBehaviour entry in data){
				if(entry is AttributeData){
					((AttributeData)entry).Validate();
				}
			}
		}
	}
	public void Start(){
		if(Application.isPlaying || Time.realtimeSinceStartup > this.nextStep){
			AttributeManager.editorInterval = this.updateInterval;
			if(!Application.isPlaying && AttributeManager.editorInterval == -1 && !AttributeManager.refresh){return;}
			this.nextStep = Time.realtimeSinceStartup + AttributeManager.editorInterval;
			if(!this.setup || AttributeManager.refresh){
				AttributeManager.refresh = false;
				this.setup = true;
				this.SceneRefresh();
			}
			this.Setup();
		}
	}
	public void Setup(){
		foreach(var attribute in Attribute.all.Copy()){
			if(attribute.info.parent.IsNull()){Attribute.all.Remove(attribute);}
		}
		foreach(var attribute in Attribute.all){attribute.BuildLookup();}
		foreach(var attribute in Attribute.all){
			attribute.BuildData(attribute.info.data);
			attribute.BuildData(attribute.info.dataB);
			attribute.BuildData(attribute.info.dataC);
		}
		Attribute.ready = true;
	}
}