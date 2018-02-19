using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios.Unity.Components.DataBehaviour{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Shortcuts;
	using Zios.SystemAttributes;
	using Zios.Unity.Call;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Locate;
	using Zios.Unity.Proxy;
	//asm Zios.Unity.Shortcuts;
	[ExecuteInEditMode][AddComponentMenu("")]
	public class DataBehaviour : MonoBehaviour{
		private static GameObject[] sorting;
		private static int processIndex;
		[Internal] public string parentPath;
		[Internal] public string path;
		public string alias;
		private string lastAlias;
		private bool alwaysDrawInspector;
		protected bool autoRename = true;
		protected bool setup;
		[NonSerialized] public List<DataDependency> dependents = new List<DataDependency>();
		[NonSerialized] public List<string> warnings = new List<string>();
		public virtual void Awake(){
			string name = this.GetType().Name.ToTitleCase();
			this.parentPath = this.gameObject.GetPath();
			this.path = this.GetPath();
			this.lastAlias = this.alias = this.alias.SetDefault(name);
			if(this.autoRename){
				while(Locate.GetObjectComponents<DataBehaviour>(this.gameObject).Exists(x=>x != this && x.alias == this.alias)){
					this.lastAlias = this.alias = this.alias.ToLetterSequence();
				}
			}
			if(!Application.isPlaying){
				Events.Register("On Destroy",this);
				Events.Register("On Validate",this);
				Events.Add("On Validate",this.CheckDependents,this);
				Events.Add("On Validate",this.CheckAlias,this);
			}
		}
		public virtual void Start(){
			this.setup = true;
			if(Application.isEditor){
				this.CheckDependents();
			}
		}
		//===============
		// Editor
		//===============
		public virtual void Reset(){
			if(Proxy.IsBusy()){return;}
			if(this.setup){
				this.CallEvent("On Reset");
				return;
			}
			Events.Call("On Attach",this);
		}
		public virtual void OnDisable(){
			if(!this.setup || Proxy.IsBusy()){return;}
			if(!this.gameObject.activeInHierarchy || !this.enabled){
				this.gameObject.DelayEvent(this.parentPath,"On Disable");
				this.gameObject.DelayEvent(this.parentPath,"On Components Changed");
			}
		}
		public virtual void OnEnable(){
			if(!this.setup || Proxy.IsBusy()){return;}
			if(!this.lastAlias.IsEmpty() && this.gameObject.activeInHierarchy && this.enabled){
				this.gameObject.DelayEvent(this.parentPath,"On Enable");
				this.gameObject.DelayEvent(this.parentPath,"On Components Changed");
			}
		}
		public virtual void OnValidate(){
			this.DelayEvent(this.path,"On Validate Raw",1);
			if(!this.CanValidate() || !this.setup){return;}
			this.DelayEvent(this.path,"On Validate",1);
			this.gameObject.DelayEvent(this.parentPath,"On Components Changed");
		}
		public virtual void OnDestroy(){
			if(Application.isPlaying || Proxy.IsBusy()){return;}
			this.CallEvent("On Destroy");
			Events.RemoveAll(this);
		}
		public virtual void CheckAlias(){
			if(this.lastAlias != this.alias || this.alias.IsEmpty()){
				Events.Call("Attribute Refresh");
				this.lastAlias = this.alias;
				this.Awake();
			}
		}
		//===============
		// Editor - Dependents
		//===============
		public virtual void CheckDependents(){
			if(!this.setup){return;}
			foreach(var dependent in this.dependents){
				var currentDependent = dependent;
				dependent.processing = false;
				dependent.exists = false;
				if(dependent.target.IsNull() && dependent.dynamicTarget.IsNull()){continue;}
				if(dependent.target.IsNull() && !dependent.dynamicTarget.Call<bool>("HasData")){continue;}
				GameObject target = dependent.target.IsNull() ? dependent.dynamicTarget.Call<GameObject>("Get") : dependent.target;
				dependent.method = ()=>{};
				if(!target.IsNull()){
					Type[] types = dependent.types;
					foreach(var type in types){
						var currentType = type;
						dependent.exists = !target.GetComponent(currentType).IsNull();
						if(dependent.exists){break;}
						dependent.method = ()=>{
							var component = target.AddComponent(currentType);
							currentDependent.processing = component != null;
						};
					}
				}
			}
		}
		public virtual void RemoveDependent<Type>() where Type : Component{this.RemoveDependent<Type>(this.gameObject);}
		public virtual void RemoveDependent<Type>(object target) where Type : Component{this.RemoveDependent(target,typeof(Type));}
		public virtual void RemoveDependent(object target,params Type[] types){this.dependents.RemoveAll(x=>Enumerable.SequenceEqual(x.types,types));}
		public virtual void AddDependent<Type>() where Type : Component{this.AddDependent<Type>(this.gameObject,true);}
		public virtual void AddDependent<Type>(object target,bool isScript=false) where Type : Component{
			this.AddDependent(target,isScript,typeof(Type));
		}
		public virtual void AddDependent(object target,bool isScript=false,params Type[] types){
			if(!Application.isEditor){return;}
			if(this.dependents.Exists(x=>Enumerable.SequenceEqual(x.types,types))){return;}
			Method delayAdd = ()=>this.DelayAddDependent(target,isScript,types);
			Events.AddLimited("On Attributes Ready",delayAdd,1);
		}
		public virtual void DelayAddDependent(object target,bool isScript=false,params Type[] types){
			if(this.dependents.Exists(x=>Enumerable.SequenceEqual(x.types,types))){return;}
			if(target.IsNull()){return;}
			var dependent = new DataDependency();
			dependent.dynamicTarget = target.GetType().Name.Contains("Attribute") ? target : null;
			dependent.target = !dependent.dynamicTarget.IsNull() ? null : target.As<GameObject>();
			dependent.types = types;
			dependent.scriptName = isScript ? this.GetType().Name : "";
			dependent.message = "[target] is missing required component : [type]. Click here to add.";
			this.dependents.AddNew(dependent);
			Call.Delay(this.CheckDependents);
		}
		//===============
		// Editor
		//===============
		#if UNITY_EDITOR
		public virtual void OnGUI(){
			if(this.alwaysDrawInspector){
				ProxyEditor.RepaintInspectors();
			}
		}
		[MenuItem("Zios/GameObject/Apply Prefab (Selected) &#P")]
		public static void ApplyPrefabSelected(){
			Debug.Log("[DataBehaviour] Applying prefab");
			foreach(var target in Selection.gameObjects){
				DataBehaviour.ApplyPrefabTarget(target);
			}
		}
		[MenuItem("Zios/GameObject/Sort Components (Selected)")]
		public static void SortSmartSelected(){
			foreach(var target in Selection.gameObjects){
				DataBehaviour.SortSmartTarget(target);
			}
		}
		[MenuItem("Zios/GameObject/Sort Components (All)")]
		public static void SortSmartAll(){
			DataBehaviour.sorting = Locate.GetSceneObjects();
			if(DataBehaviour.sorting.Length > 0){
				DataBehaviour.processIndex = 0;
				Events.Add("On Editor Update",DataBehaviour.SortSmartNext);
			}
		}
		public static void SortSmartNext(){
			Events.Pause("On Hierarchy Changed");
			int index = DataBehaviour.processIndex;
			var sorting = DataBehaviour.sorting;
			bool canceled = true;
			if(index < DataBehaviour.sorting.Length-1){
				var current = DataBehaviour.sorting[index];
				float total = (float)index/sorting.Length;
				string message = index + " / " + sorting.Length + " -- " + current.GetPath();
				canceled = EditorUI.DrawProgressBar("Sorting All Components",message,total);
				current.PauseValidate();
				DataBehaviour.SortSmartTarget(current);
				current.ResumeValidate();
			}
			DataBehaviour.processIndex += 1;
			if(canceled || index+1 > sorting.Length-1){
				EditorUI.ClearProgressBar();
				Events.Remove("On Editor Update",DataBehaviour.SortSmartNext);
				Events.Resume("On Hierarchy Changed");
			}
		}
		//===============
		// Shared
		//===============
		public static void SortSmartTarget(GameObject target){
			Component[] components = target.GetComponents<Component>().ToList().OrderBy(x=>x.GetAlias()).ToArray();
			DataBehaviour.Sort(components);
			var controller = components.Find(x=>x.GetType().Name.Contains("State"));
			if(!controller.IsNull()){controller.MoveToTop();}
		}
		public static void Sort(Component[] components){
			foreach(var component in components){
				if(!component.hideFlags.Contains(HideFlags.HideInInspector)){
					component.MoveToBottom();
				}
			}
			foreach(var component in components){
				if(component.hideFlags.Contains(HideFlags.HideInInspector)){
					component.MoveToBottom();
				}
			}
		}
		public static void ApplyPrefabTarget(GameObject target){
			Events.Pause("On Hierarchy Changed");
			target.PauseValidate();
			ProxyEditor.ApplyPrefab(target);
			target.ResumeValidate();
			Events.Resume("On Hierarchy Changed");
		}
		//===============
		// Context
		//===============
		[ContextMenu("Sort (By Type)")]
		public void SortByType(){
			Component[] components = this.GetComponents<Component>().ToList().OrderBy(x=>x.GetType().Name).ToArray();
			DataBehaviour.Sort(components);
		}
		[ContextMenu("Sort (By Alias)")]
		public void SortByAlias(){
			Component[] components = this.GetComponents<Component>().ToList().OrderBy(x=>x.GetAlias()).ToArray();
			DataBehaviour.Sort(components);
		}
		[ContextMenu("Sort (Smart)")]
		public void SortSmart(){DataBehaviour.SortSmartTarget(this.gameObject);}
		[ContextMenu("Move Element Up")]
		public void MoveItemUp(){this.MoveUp();}
		[ContextMenu("Move Element Down")]
		public void MoveItemDown(){this.MoveDown();}
		[ContextMenu("Move To Bottom")]
		public void MoveBottom(){this.MoveToBottom();}
		[ContextMenu("Move To Top")]
		public void MoveTop(){this.MoveToTop();}
		[ContextMenu("Apply Prefab")]
		public void ApplyPrefab(){DataBehaviour.ApplyPrefabTarget(this.gameObject);}
		[ContextMenu("Always Draw Inspector")]
		public void AlwaysDraw(){this.alwaysDrawInspector = !this.alwaysDrawInspector;}
		#endif
	}
	public class DataDependency{
		public bool exists;
		public bool processing;
		public string scriptName;
		public object dynamicTarget;
		public GameObject target;
		public Type[] types;
		public string message;
		public Method method = ()=>{};
	}
}