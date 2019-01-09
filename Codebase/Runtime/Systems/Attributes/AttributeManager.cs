using UnityEngine;
namespace Zios.Attributes{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.Call;
	using Zios.Unity.Components.DataBehaviour;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Locate;
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	using Zios.Unity.Supports.Singleton;
	using Zios.Unity.Time;
	//asm Zios.Shortcuts;
	//asm Zios.Supports.Hierarchy;
	//asm Zios.Unity.Shortcuts;
	public class AttributeManager : Singleton{
		public static AttributeManager singleton;
		public static float nextRefresh;
		public static float percentLoaded;
		public bool disabled = false;
		public int editorRefreshPasses = -1;
		public bool editorIncludeDisabled = true;
		public bool refreshOnComponentsChanged = true;
		public bool safeMode = true;
		private float start;
		private float block;
		private DataBehaviour[] data = new DataBehaviour[0];
		private int nextIndex;
		private int stage;
		//==============================
		// Editor
		//==============================
		[ContextMenu("Refresh")]
		public void ContextRefresh(){
			AttributeManager.PerformRefresh();
		}
		public static void PerformRefresh(){AttributeManager.Refresh();}
		public static void Refresh(int delay = 0){
			if(Proxy.IsPlaying() || AttributeManager.Get().disabled){return;}
			Events.Call("On Attributes Refresh");
			AttributeManager.nextRefresh = Time.Get() + delay;
		}
		//==============================
		// Unity
		//==============================
		public static AttributeManager Get(){
			AttributeManager.singleton = AttributeManager.singleton ?? Singleton.Get<AttributeManager>();
			return AttributeManager.singleton;
		}
		public void OnValidate(){this.SetupEvents();}
		public void OnEnable(){
			this.Setup();
			AttributeManager.Refresh();
			this.EditorUpdate();
		}
		public void EditorUpdate(){
			if(AttributeManager.Get().disabled){return;}
			if(AttributeManager.nextRefresh > 0 && Time.Get() > AttributeManager.nextRefresh){
				if(Attribute.debug.Has("ProcessRefresh")){Log.Editor("[AttributeManager] Refreshing...");}
				this.Setup();
			}
			if(this.editorRefreshPasses < 1){
				if(!Attribute.ready){
					this.Setup();
					this.SceneRefresh();
					if(Attribute.debug.Has("ProcessStage")){Log.Editor("[AttributeManager] Stage 1 (Awake) start...");}
					Events.Call("On Attributes Setup");
					this.block = Time.Get();
					while(this.stage != 0){this.Process();}
				}
			}
			else if(this.stage != 0){
				for(int index=1;index<=this.editorRefreshPasses;++index){
					Call.Delay(this.Process);
				}
			}
		}
		public void OnDisable(){Events.RemoveAll(this);}
		//==============================
		// Main
		//==============================
		public void Setup(){
			this.stage = 1;
			this.nextIndex = 0;
			Attribute.ready = false;
			AttributeManager.singleton = this;
			AttributeManager.nextRefresh = 0;
			this.SetupEvents();
			this.SetupHooks();
		}
		public void SetupEvents(){
			if(!Proxy.IsPlaying()){
				Events.Register("On Attribute Setup");
				Events.Register("On Attribute Ready");
				Events.Register("On Attribute Refresh");
				Events.Remove("On Components Changed",AttributeManager.PerformRefresh);
				Events.Add("On Events Reset",AttributeManager.PerformRefresh);
				//if(this.refreshOnHierarchyChanged){Event.Add("On Hierarchy Changed",AttributeManager.PerformRefresh);}
				if(this.refreshOnComponentsChanged){Events.Add("On Components Changed",AttributeManager.PerformRefresh);}
			}
			Events.Add("On Level Was Loaded",this.OnEnable);
			Events.Add("On Editor Update",this.EditorUpdate);
			Events.Add("On Validate",this.SetupEvents,this);
			Events.Add("Attribute Refresh",AttributeManager.PerformRefresh);
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
			bool fullSweep = !Proxy.IsPlaying();
			if(fullSweep){
				Attribute.all.Clear();
				Attribute.lookup.Clear();
			}
			bool includeEnabled = Attribute.ready || !Proxy.IsPlaying();
			bool includeDisabled = !Attribute.ready || this.editorIncludeDisabled;
			if(Attribute.debug.Has("ProcessRefresh")){Log.Editor("[AttributeManager] Scene Refreshing...");}
			this.data = Locate.GetSceneComponents<DataBehaviour>(includeEnabled,includeDisabled);
			if(Attribute.debug.Has("ProcessRefresh")){Log.Editor("[AttributeManager] DataBehaviour Count : " + this.data.Length);}
			this.start = Time.Get();
			this.nextIndex = 0;
		}
		public void DisplayStageTime(string message){
			string duration = (Time.Get() - this.block) + " seconds.";
			Log.Editor(message + " " + duration);
			this.block = Time.Get();
		}
		public void StepAwake(){
			if(this.nextIndex > this.data.Length-1){
				this.stage = 2;
				this.nextIndex = 0;
				if(Attribute.debug.Has("ProcessTime")){this.DisplayStageTime("[AttributeManager] Stage 1 (Awake)");}
				if(Attribute.debug.Has("ProcessStage")){Log.Editor("[AttributeManager] Stage 2 (Build Lookup) start...");}
				return;
			}
			if(!this.data[this.nextIndex].IsNull()){
				this.data[this.nextIndex].Awake();
			}
			else if(Attribute.debug.Has("Issue")){Log.Editor("[AttributeManager] Stage 1 (Awake) index " + this.nextIndex + " was null.");}
			this.nextIndex += 1;
		}
		public void StepBuildLookup(){
			if(this.nextIndex > Attribute.all.Count-1){
				this.stage = 3;
				this.nextIndex = 0;
				if(Attribute.debug.Has("ProcessTime")){this.DisplayStageTime("[AttributeManager] Stage 2 (Build Lookup)");}
				if(Attribute.debug.Has("ProcessStage")){Log.Editor("[AttributeManager] Stage 3 (Build Data) start...");}
				return;
			}
			var attribute = Attribute.all[this.nextIndex];
			if(attribute.IsNull() || attribute.info.parent.IsNull()){
				if(Attribute.debug.Has("Issue")){Log.Editor("[AttributeManager] Null attribute found.  Removing index " + this.nextIndex + ".");}
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
						Log.Editor("[AttributeManager] Refresh Complete : " + (Time.Get() - this.start) + " seconds.");
					}
				}
				Attribute.ready = true;
				AttributeManager.percentLoaded = 1;
				ProxyEditor.RepaintInspectors();
				Events.Call("On Attributes Ready");
				Events.Rest("On Attributes Refresh",1);
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