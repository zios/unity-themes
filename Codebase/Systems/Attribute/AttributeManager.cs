using Zios;
using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
	public class AttributeManager : MonoBehaviour{
		private static float nextRefresh;
		public static float percentLoaded;
		public static bool refresh;
		public static bool safe = true;
		public static bool preDrawn = true;
		public static bool debug = false;
		public int editorRefreshPasses = -1;
		public bool editorIncludeDisabled = true;
		public bool refreshOnHierarchyChange = false;
		public bool refreshOnAssetChange = false;
		public bool preDraw = true;
		public bool safeMode = true;
		public bool debugMode = true;
		private float start;
		private DataMonoBehaviour[] data = new DataMonoBehaviour[0];
		private int nextIndex;
		private int stage;
		//==============================
		// Editor
		//==============================
		#if UNITY_EDITOR
		[MenuItem("Zios/Process/Attribute/Remove Visible Data")]
		public static void RemoveVisibleData(){AttributeManager.RemoveAttributeData(true);}
		[MenuItem("Zios/Process/Attribute/Remove All Data")]
		public static void RemoveAttributeData(bool visibleOnly=false){
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
		[MenuItem("Zios/Process/Attribute/Hide All Data %3")]
		public static void HideAttributeData(){
			PlayerPrefs.SetInt("ShowAttributeData",0);
			AttributeManager.refresh = true;
		}
		[MenuItem("Zios/Process/Attribute/Show All Data %2")]
		public static void ShowAttributeData(){
			PlayerPrefs.SetInt("ShowAttributeData",1);
			AttributeManager.refresh = true;
		}
		[MenuItem("Zios/Process/Attribute/Full Refresh %1")]
		public static void PerformRefresh(){
			AttributeManager.nextRefresh = Time.realtimeSinceStartup + 1;
		}
		[ContextMenu("Refresh")]
		public void ContextRefresh(){
			AttributeManager.PerformRefresh();
		}
		#endif
		public void CleanEvents(){
			Utility.RemoveAssetUpdate(AttributeManager.PerformRefresh);
			Utility.RemoveHierarchyUpdate(AttributeManager.PerformRefresh);
			Utility.RemoveEditorUpdate(this.Start);
		}
		//==============================
		// Unity
		//==============================
		public void OnValidate(){this.CleanEvents();}
		public void OnDestroy(){this.CleanEvents();}
		public void OnApplicationQuit(){AttributeManager.PerformRefresh();}
		public void Update(){
			if(this.refreshOnAssetChange){Utility.AddAssetUpdate(AttributeManager.PerformRefresh);}
			if(this.refreshOnHierarchyChange){Utility.AddHierarchyUpdate(AttributeManager.PerformRefresh);}
			Utility.AddEditorUpdate(this.Start);
		}
		//==============================
		// Main
		//==============================
		public void Start(){
			AttributeManager.safe = this.safeMode;
			AttributeManager.debug = this.debugMode;
			AttributeManager.preDrawn = this.preDraw;
			if(AttributeManager.nextRefresh > 0 && Time.realtimeSinceStartup > AttributeManager.nextRefresh){
				if(this.debugMode){Utility.EditorLog("[AttributeManager] Refreshing...");}
				Locate.SetDirty();
				Attribute.ready = false;
				AttributeManager.refresh = true;
				AttributeManager.nextRefresh = 0;
			}
			if(this.editorRefreshPasses < 1){
				if(AttributeManager.refresh || !Attribute.ready){
					this.SceneRefresh();
					AttributeManager.refresh = false;
					this.stage = 1;
					if(this.debugMode){Utility.EditorLog("[AttributeManager] Stage 1 (Awake) start...");}
					while(this.stage != 0){this.Process();}
				}
			}
			else{
				for(int index=1;index<=this.editorRefreshPasses;++index){
					Utility.EditorDelayCall(this.Process);
				}
			}	
		}
		public void Process(){
			if(this.stage > 0){
				if(this.stage == 1){this.StepAwake();}
				if(this.stage == 2){this.StepBuildLookup();}
				if(this.stage == 3){this.StepBuildData();}
				AttributeManager.percentLoaded = (((float)this.nextIndex / this.data.Length) / 4.0f) + ((this.stage-1)*0.25f);
			}
		}
		public void SceneRefresh(){
			bool fullSweep = !Application.isPlaying;
			if(fullSweep){
				Attribute.all.Clear();
				Attribute.lookup.Clear();
			}
			bool includeEnabled = Attribute.ready || !Application.isPlaying;
			bool includeDisabled = !Attribute.ready || this.editorIncludeDisabled;
			if(this.debugMode){Utility.EditorLog("[AttributeManager] Scene Refreshing...");}
			this.data = Locate.GetSceneComponents<DataMonoBehaviour>(includeEnabled,includeDisabled);
			if(this.debugMode){Utility.EditorLog("[AttributeManager] DataMonoBehaviour Count : " + this.data.Length);}
			this.start = Time.realtimeSinceStartup;
			this.nextIndex = 0;
		}
		public void StepAwake(){
			if(this.nextIndex > this.data.Length-1){
				this.stage = 2;
				this.nextIndex = 0;
				if(!Application.isPlaying){
					if(this.debugMode){Utility.EditorLog("[AttributeManager] Stage 1b (Validate) start...");}
					foreach(DataMonoBehaviour entry in this.data){
						if(!entry.IsNull() && entry is AttributeData){
							((AttributeData)entry).Validate();
						}
					}
				}
				if(this.debugMode){Utility.EditorLog("[AttributeManager] Stage 2 (Build Lookup) start...");}
				return;
			}
			if(!this.data[this.nextIndex].IsNull()){
				this.data[this.nextIndex].Awake();
			}
			else if(this.debugMode){Utility.EditorLog("[AttributeManager] Stage 1 (Awake) index " + this.nextIndex + " was null.");}
			this.nextIndex += 1;
			if(AttributeManager.refresh){
				if(this.debugMode){Utility.EditorLog("[AttributeManager] Resetting process due to refresh during Awake.");}
				this.stage = 0;
				this.nextIndex = 0;
			}
		}
		public void StepBuildLookup(){
			if(this.nextIndex > Attribute.all.Count-1){
				this.stage = 3;
				this.nextIndex = 0;
				if(this.debugMode){Utility.EditorLog("[AttributeManager] Stage 3 (Build Data) start...");}
				return;
			}
			var attribute = Attribute.all[this.nextIndex];
			if(attribute.IsNull() || attribute.info.parent.IsNull()){
				if(this.debugMode){Utility.EditorLog("[AttributeManager] Null attribute found.  Removing index " + this.nextIndex + ".");}
				Attribute.all.Remove(attribute);
				return;
			}
			attribute.BuildLookup();
			this.nextIndex += 1;
		}
		public void StepBuildData(){
			if(this.nextIndex > Attribute.all.Count-1){
				if(!Attribute.ready && this.debugMode){
					Utility.EditorLog("[AttributeManager] Refresh Complete : " + (Time.realtimeSinceStartup - this.start) + " seconds.");
					Utility.EditorLog("[AttributeManager] AttributeData Count : " + this.data.Count(x=>x is AttributeData));
				}
				Attribute.ready = true;
				AttributeManager.percentLoaded = 1;
				this.stage = 0;
				this.nextIndex = 0;
				return;
			}
			var attribute = Attribute.all[this.nextIndex];		
			attribute.BuildData(attribute.info.data);
			attribute.BuildData(attribute.info.dataB);
			attribute.BuildData(attribute.info.dataC);
			this.nextIndex += 1;
		}
	}
}