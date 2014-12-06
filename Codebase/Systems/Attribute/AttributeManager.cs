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
	private static bool refresh;
	public float editorInterval = 1;
	public bool editorIncludeDisabled = true;
	public bool updateOnHierarchyChange = true;
	public bool updateOnAssetChange = true;
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
		bool includeEnabled = this.setup;
		bool includeDisabled = !this.setup || this.editorIncludeDisabled;
		DataMonoBehaviour[] data = Locate.GetSceneComponents<DataMonoBehaviour>(includeEnabled,includeDisabled);
		//data = data.OrderBy(x=>x.GetType().ToString()).ToArray();
		foreach(DataMonoBehaviour entry in data){entry.Awake();}
		this.Setup();
	}
	public void Start(){
		if(Application.isPlaying || Time.realtimeSinceStartup > this.nextStep){
			if(!Application.isPlaying && this.editorInterval == -1){return;}
			this.nextStep = Time.realtimeSinceStartup + this.editorInterval;
			if(!this.setup || AttributeManager.refresh){
				this.SceneRefresh();
				this.setup = true;
				AttributeManager.refresh = false;
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