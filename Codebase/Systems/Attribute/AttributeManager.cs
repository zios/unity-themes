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
		public static float percentLoaded;
		public static bool refresh;
		public static float editorInterval = 1;
		public static bool safe = true;
		public static bool preDrawn = true;
		public static bool debug = false;
		public float updateInterval = 1;
		public int editorRefreshPasses = -1;
		public bool editorIncludeDisabled = true;
		public bool updateOnHierarchyChange = false;
		public bool updateOnAssetChange = false;
		public bool preDraw = true;
		public bool safeMode = true;
		public bool debugMode = true;
		private float nextStep;
		private float start;
		private DataMonoBehaviour[] data = new DataMonoBehaviour[0];
		private int nextIndex;
		private bool activeRefresh;
		private bool setup;
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
		public static void AttributeRefresh(){
			Attribute.ready = false;
			AttributeManager.refresh = true;
		}	
		#endif
		public void CleanEvents(){
			Utility.RemoveAssetUpdate(this.PerformRefresh);
			Utility.RemoveHierarchyUpdate(this.PerformRefresh);
			Utility.RemoveEditorUpdate(this.Start);
		}
		[ContextMenu("Refresh")]
		public void PerformRefresh(){
			Locate.SetDirty();
			Attribute.ready = false;
			AttributeManager.refresh = true;
		}
		//==============================
		// Unity
		//==============================
		public void OnValidate(){this.CleanEvents();}
		public void OnDestroy(){this.CleanEvents();}
		public void OnApplicationQuit(){this.PerformRefresh();}
		public void Update(){
			if(this.updateOnAssetChange){Utility.AddAssetUpdate(this.PerformRefresh);}
			if(this.updateOnHierarchyChange){Utility.AddHierarchyUpdate(this.PerformRefresh);}
			Utility.AddEditorUpdate(this.Start);
		}
		//==============================
		// Main
		//==============================
		public void Start(){
			bool editor = !Application.isPlaying;
			AttributeManager.safe = this.safeMode;
			AttributeManager.debug = this.debugMode;
			AttributeManager.preDrawn = this.preDraw;
			if(this.activeRefresh || AttributeManager.editorInterval != -1){
				if(!editor && !this.setup){
					this.SceneRefresh();
					this.setup = true;
					this.stage = 1;
					while(this.stage != 0){this.Start();}
					return;
				}
				if(editor && this.stage != 0 && !this.activeRefresh && this.editorRefreshPasses > 1){
					for(int index=0;index<this.editorRefreshPasses-1;++index){
						Utility.EditorDelayCall(this.StartStep);
					}
				}
				if(!AttributeManager.refresh){
					if(this.stage == 1){this.StepRefresh();}
					if(this.stage == 2){this.StepBuildLookup();}
					if(this.stage == 3){this.StepBuildData();}
					AttributeManager.percentLoaded = (((float)this.nextIndex / this.data.Length) / 4.0f) + ((this.stage-1)*0.25f);
				}
			}
			if(editor && Time.realtimeSinceStartup > this.nextStep){
				AttributeManager.editorInterval = this.updateInterval;
				if(AttributeManager.editorInterval == -1 && !AttributeManager.refresh){return;}
				this.nextStep = Time.realtimeSinceStartup + AttributeManager.editorInterval;
				if(!this.setup || AttributeManager.refresh){
					AttributeManager.refresh = false;
					this.setup = true;
					this.SceneRefresh();
					this.stage = 1;
					if(this.debugMode){Utility.EditorLog("[AttributeManager] Refreshing...");}
					if(this.editorRefreshPasses <= 0 && !this.activeRefresh){
						this.activeRefresh = true;
						while(this.stage != 0){this.Start();}
						this.activeRefresh = false;
					}
				}
				else if(this.stage == 0){
					this.stage = 2;
				}
			}
		}
		public void StartStep(){
			this.activeRefresh = true;
			this.Start();
			this.activeRefresh = false;
		}
		public void SceneRefresh(){
			bool fullSweep = !Application.isPlaying;
			if(fullSweep){
				Attribute.all.Clear();
				Attribute.lookup.Clear();
			}
			bool includeEnabled = this.setup || !Application.isPlaying;
			bool includeDisabled = !this.setup || this.editorIncludeDisabled;
			//Locate.SetDirty();
			this.data = Locate.GetSceneComponents<DataMonoBehaviour>(includeEnabled,includeDisabled);
			//this.data = this.data.OrderBy(x=>x.GetType().ToString()).ToArray();
			this.start = Time.realtimeSinceStartup;
			this.nextIndex = 0;
		}
		public void StepRefresh(){
			if(this.nextIndex > this.data.Length-1){
				this.stage = 2;
				if(!Application.isPlaying){
					foreach(DataMonoBehaviour entry in this.data){
						if(!entry.IsNull() && entry is AttributeData){
							((AttributeData)entry).Validate();
						}
					}
				}
				return;
			}
			if(!this.data[this.nextIndex].IsNull()){
				this.data[this.nextIndex].Awake();
			}
			this.nextIndex += 1;
			if(AttributeManager.refresh){
				this.stage = 0;
				this.nextIndex = 0;
			}
		}
		public void StepBuildLookup(){
			if(this.nextIndex > Attribute.all.Count-1){
				this.stage = 3;
				this.nextIndex = 0;
				return;
			}
			var attribute = Attribute.all[this.nextIndex];
			if(attribute.IsNull() || attribute.info.parent.IsNull()){
				Attribute.all.Remove(attribute);
				return;
			}
			attribute.BuildLookup();
			this.nextIndex += 1;
		}
		public void StepBuildData(){
			if(this.nextIndex > Attribute.all.Count-1){
				if(!Attribute.ready){
					if(this.debugMode){Utility.EditorLog("[AttributeManager] Refresh Complete : " + (Time.realtimeSinceStartup - this.start) + " seconds.");}
					if(this.debugMode){Utility.EditorLog("[AttributeManager] Data Count : " + this.data.Count(x=>x is AttributeData));}
				}
				Attribute.ready = true;
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