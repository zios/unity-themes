using Zios;
using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using UnityObject = UnityEngine.Object;
namespace Zios{
	#if UNITY_EDITOR
	using UnityEditor;
	[InitializeOnLoad]
	public static class AttributeManagerHook{
		static private bool setup;
		static AttributeManagerHook(){
			if(Application.isPlaying){return;}
			EditorApplication.delayCall += ()=>{
				AttributeManagerHook.Create();
				Events.Add("On Hierarchy Changed",AttributeManagerHook.Reset).SetPermanent();
				Events.Add("On Scene Loaded",AttributeManagerHook.Reset).SetPermanent();
			};
		}
		public static void Reset(){
			AttributeManagerHook.setup = false;
			AttributeManagerHook.Create();
		}
		public static void Create(){
			if(AttributeManagerHook.setup || Application.isPlaying){return;}
			AttributeManagerHook.setup = true;
			if(AttributeManager.instance.IsNull()){
				var path = Locate.GetScenePath("@Main");
				if(!path.HasComponent<AttributeManager>()){
					Debug.Log("[AttributeManager] : Auto-creating Attribute Manager GameObject.");
					AttributeManager.instance = path.AddComponent<AttributeManager>();
				}
				AttributeManager.instance = path.GetComponent<AttributeManager>();
			}
			Events.Add("On Editor Update",AttributeManager.instance.EditorUpdate);
		}
	}
	#endif
	[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
	public class AttributeManager : MonoBehaviour{
		public static AttributeManager instance;
		public static bool disabled = false;
		[NonSerialized] public static float nextRefresh;
		[NonSerialized] public static float percentLoaded;
		[NonSerialized] public static bool safe = false;
		public int editorRefreshPasses = -1;
		public bool editorIncludeDisabled = true;
		public bool refreshOnHierarchyChange = true;
		public bool refreshOnAssetChange = true;
		public bool safeMode = true;
		private float start;
		private float block;
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
			var objects = Locate.GetSceneObjects();
			foreach(UnityObject current in objects){
				GameObject gameObject = (GameObject)current;
				foreach(AttributeData data in Locate.GetObjectComponents<AttributeData>(gameObject)){
					bool canDestroy = !visibleOnly || (visibleOnly && !data.hideFlags.Contains(HideFlags.HideInInspector));
					if(canDestroy){
						Utility.Destroy(data);
					}
				}
			}
		}
		[MenuItem("Zios/Process/Attribute/Hide All Data %3")]
		public static void HideAttributeData(){
			Debug.Log("[AttributeManager] Hiding AttributeData.");
			PlayerPrefs.SetInt("Attribute-ShowData",0);
			AttributeManager.PerformRefresh();
		}
		[MenuItem("Zios/Process/Attribute/Show All Data %2")]
		public static void ShowAttributeData(){
			Debug.Log("[AttributeManager] Unhiding AttributeData.");
			PlayerPrefs.SetInt("Attribute-ShowData",1);
			AttributeManager.PerformRefresh();
		}
		[ContextMenu("Refresh")]
		public void ContextRefresh(){
			AttributeManager.PerformRefresh();
		}
		[MenuItem("Zios/Process/Attribute/Full Refresh %1")]
		#endif
		public static void PerformRefresh(){
			if(Application.isPlaying || AttributeManager.disabled){return;}
			Events.Call("On Attributes Refresh");
			AttributeManager.nextRefresh = Time.realtimeSinceStartup + 1;

		}
		//==============================
		// Unity
		//==============================
		public void Awake(){this.EditorUpdate();}
		public void EditorUpdate(){
			if(AttributeManager.disabled){return;}
			if(AttributeManager.nextRefresh > 0 && Time.realtimeSinceStartup > AttributeManager.nextRefresh){
				if(Attribute.debug.Has("ProcessRefresh")){Utility.EditorLog("[AttributeManager] Refreshing...");}
				this.Setup();
			}
			if(this.editorRefreshPasses < 1){
				if(!Attribute.ready){
					this.Setup();
					this.SceneRefresh();
					if(Attribute.debug.Has("ProcessStage")){Utility.EditorLog("[AttributeManager] Stage 1 (Awake) start...");}
					Events.Call("On Attributes Setup");
					this.block = Time.realtimeSinceStartup;
					while(this.stage != 0){this.Process();}
				}
			}
			else if(this.stage != 0){
				for(int index=1;index<=this.editorRefreshPasses;++index){
					Utility.EditorDelayCall(this.Process);
				}
			}
		}
		//==============================
		// Main
		//==============================
		public void Setup(){
			this.stage = 1;
			this.nextIndex = 0;
			Locate.SetDirty();
			Attribute.ready = false;
			AttributeManager.instance = this;
			AttributeManager.safe = this.safeMode;
			AttributeManager.nextRefresh = 0;
			if(!Application.isPlaying){
				Events.Register("On Attribute Setup");
				Events.Register("On Attribute Ready");
				Events.Register("On Attribute Refresh");
				if(this.refreshOnAssetChange){Events.Add("On Asset Changed",AttributeManager.PerformRefresh);}
				if(this.refreshOnHierarchyChange){Events.Add("On Hierarchy Changed",AttributeManager.PerformRefresh);}
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
			if(Attribute.debug.Has("ProcessRefresh")){Utility.EditorLog("[AttributeManager] Scene Refreshing...");}
			this.data = Locate.GetSceneComponents<DataMonoBehaviour>(includeEnabled,includeDisabled);
			if(Attribute.debug.Has("ProcessRefresh")){Utility.EditorLog("[AttributeManager] DataMonoBehaviour Count : " + this.data.Length);}
			this.start = Time.realtimeSinceStartup;
			this.nextIndex = 0;
		}
		public void DisplayStageTime(string message){
			string duration = (Time.realtimeSinceStartup - this.block) + " seconds.";
			Utility.EditorLog(message + " " + duration);
			this.block = Time.realtimeSinceStartup;
		}
		public void StepAwake(){
			if(this.nextIndex > this.data.Length-1){
				this.stage = 2;
				this.nextIndex = 0;
				if(!Application.isPlaying){
					if(Attribute.debug.Has("ProcessTime")){this.DisplayStageTime("[AttributeManager] Stage 1 (Awake)");}
					if(Attribute.debug.Has("ProcessStage")){Utility.EditorLog("[AttributeManager] Stage 1b (Validate) start...");}
					foreach(DataMonoBehaviour entry in this.data){
						if(!entry.IsNull() && entry is AttributeData){
							((AttributeData)entry).Purge();
						}
					}
				}
				if(Attribute.debug.Has("ProcessTime")){this.DisplayStageTime("[AttributeManager] Stage 1b (Validate)");}
				if(Attribute.debug.Has("ProcessStage")){Utility.EditorLog("[AttributeManager] Stage 2 (Build Lookup) start...");}
				return;
			}
			if(!this.data[this.nextIndex].IsNull()){
				this.data[this.nextIndex].Awake();
			}
			else if(Attribute.debug.Has("Issue")){Utility.EditorLog("[AttributeManager] Stage 1 (Awake) index " + this.nextIndex + " was null.");}
			this.nextIndex += 1;
		}
		public void StepBuildLookup(){
			if(this.nextIndex > Attribute.all.Count-1){
				this.stage = 3;
				this.nextIndex = 0;
				if(Attribute.debug.Has("ProcessTime")){this.DisplayStageTime("[AttributeManager] Stage 2 (Build Lookup)");}
				if(Attribute.debug.Has("ProcessStage")){Utility.EditorLog("[AttributeManager] Stage 3 (Build Data) start...");}
				return;
			}
			var attribute = Attribute.all[this.nextIndex];
			if(attribute.IsNull() || attribute.info.parent.IsNull()){
				if(Attribute.debug.Has("Issue")){Utility.EditorLog("[AttributeManager] Null attribute found.  Removing index " + this.nextIndex + ".");}
				Attribute.all.Remove(attribute);
				return;
			}
			attribute.BuildLookup();
			this.nextIndex += 1;
		}
		public void StepBuildData(){
			if(this.nextIndex > Attribute.all.Count-1){
				if(!Attribute.ready){
					if(Attribute.debug.Has("ProcessTime")){
						this.DisplayStageTime("[AttributeManager] Stage 3 (Build Data)");
						Utility.EditorLog("[AttributeManager] Refresh Complete : " + (Time.realtimeSinceStartup - this.start) + " seconds.");
					}
					if(Attribute.debug.Has("ProcessRefresh")){
						Utility.EditorLog("[AttributeManager] AttributeData Count : " + this.data.Count(x=>x is AttributeData));
					}
				}
				Utility.RepaintInspectors();
				Attribute.ready = true;
				AttributeManager.percentLoaded = 1;
				Events.Call("On Attributes Ready");
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
