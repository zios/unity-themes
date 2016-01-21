#pragma warning disable 0618
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
	[ExecuteInEditMode][AddComponentMenu("")]
	public class DataMonoBehaviour : MonoBehaviour{
		private static GameObject[] sorting;
		private static int processIndex;
		[Internal] public string parentPath;
		[Internal] public string path;
		public string alias;
		private string lastAlias;
		protected bool autoRename = true;
		protected bool setup;
		[NonSerialized] public List<DataDependency> dependents = new List<DataDependency>();
		public virtual void Awake(){
			string name = this.GetType().Name.ToTitle();
			this.parentPath = this.gameObject.GetPath();
			this.path = this.GetPath();
			this.lastAlias = this.alias = this.alias.SetDefault(name);
			if(this.autoRename){
				while(Locate.GetObjectComponents<DataMonoBehaviour>(this.gameObject).Exists(x=>x != this && x.alias == this.alias)){
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
			this.CheckDependents();
		}
		//===============
		// Editor
		//===============
		public virtual void Reset(){
			if(Utility.IsBusy()){return;}
			if(this.setup){
				this.CallEvent("On Reset");
				return;
			}
			Events.Call("On Attach",this);
		}
		public virtual void OnDisable(){
			if(!this.setup || Utility.IsBusy()){return;}
			if(!this.gameObject.activeInHierarchy || !this.enabled){
				this.gameObject.DelayEvent(this.parentPath,"On Disable");
				this.gameObject.DelayEvent(this.parentPath,"On Components Changed");
			}
		}
		public virtual void OnEnable(){
			if(!this.setup || Utility.IsBusy()){return;}
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
			if(Application.isPlaying || Utility.IsBusy()){return;}
			this.CallEvent("On Destroy");
			Events.RemoveAll(this);
		}
		public void CheckAlias(){
			if(this.lastAlias != this.alias || this.alias.IsEmpty()){
				AttributeManager.PerformRefresh();
				this.lastAlias = this.alias;
				this.Awake();
			}
		}
		public void CheckDependents(){
			if(!this.setup){return;}
			foreach(var dependent in this.dependents){
				var currentDependent = dependent;
				dependent.processing = false;
				dependent.exists = false;
				if(dependent.target.IsNull() && dependent.dynamicTarget.IsNull()){continue;}
				if(dependent.target.IsNull() && !dependent.dynamicTarget.HasData()){continue;}
				GameObject target = dependent.target.IsNull() ? dependent.dynamicTarget.Get() : dependent.target;
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
		//===============
		// Editor - Dependents
		//===============
		public void RemoveDependent<Type>() where Type : Component{this.RemoveDependent<Type>(this.gameObject);}
		public void RemoveDependent<Type>(object target) where Type : Component{this.RemoveDependent(target,typeof(Type));}
		public void RemoveDependent(object target,params Type[] types){this.dependents.RemoveAll(x=>Enumerable.SequenceEqual(x.types,types));}
		public void AddDependent<Type>() where Type : Component{this.AddDependent<Type>(this.gameObject,true);}
		public void AddDependent<Type>(object target,bool isScript=false) where Type : Component{
			this.AddDependent(target,isScript,typeof(Type));
		}
		public void AddDependent(object target,bool isScript=false,params Type[] types){
			if(!Application.isEditor){return;}
			if(this.dependents.Exists(x=>Enumerable.SequenceEqual(x.types,types))){return;}
			Method delayAdd = ()=>this.DelayAddDependent(target,isScript,types);
			Events.AddLimited("On Attributes Ready",delayAdd,1);
		}
		public void DelayAddDependent(object target,bool isScript=false,params Type[] types){
			if(this.dependents.Exists(x=>Enumerable.SequenceEqual(x.types,types))){return;}
			if(target.IsNull()){return;}
			var dependent = new DataDependency();
			dependent.dynamicTarget = target is AttributeGameObject ? (AttributeGameObject)target : null;
			dependent.target = target is AttributeGameObject ? null : (GameObject)target;
			dependent.types = types;
			dependent.scriptName = isScript ? this.GetType().Name : "";
			dependent.message = "[target] is missing required component : [type]. Click here to add.";
			this.dependents.AddNew(dependent);
			Utility.DelayCall(this.CheckDependents);
		}
		//===============
		// Editor - Sorting
		//===============
		#if UNITY_EDITOR
		[MenuItem("Zios/Process/GameObject/Apply Prefab (Selected) %4")]
		public static void ApplyPrefabSelected(){
			foreach(var target in Selection.gameObjects){
				DataMonoBehaviour.ApplyPrefabTarget(target);
			}
		}
		[MenuItem("Zios/Process/GameObject/Sort Components (Selected)")]
		public static void SortSmartSelected(){
			foreach(var target in Selection.gameObjects){
				DataMonoBehaviour.SortSmartTarget(target);
			}
		}
		[MenuItem("Zios/Process/GameObject/Sort Components (All)")]
		public static void SortSmartAll(){
			DataMonoBehaviour.sorting = Locate.GetSceneObjects();
			if(DataMonoBehaviour.sorting.Length > 0){
				DataMonoBehaviour.processIndex = 0;
				Events.Add("On Editor Update",DataMonoBehaviour.SortSmartNext);
			}
		}
		public static void SortSmartNext(){
			Events.Pause("On Hierarchy Changed");
			int index = DataMonoBehaviour.processIndex;
			var sorting = DataMonoBehaviour.sorting;
			bool canceled = true;
			if(index < DataMonoBehaviour.sorting.Length-1){
				var current = DataMonoBehaviour.sorting[index];
				float total = (float)index/sorting.Length;
				string message = index + " / " + sorting.Length + " -- " + current.GetPath();
				canceled = EditorUtility.DisplayCancelableProgressBar("Sorting All Components",message,total);
				current.PauseValidate();
				DataMonoBehaviour.SortSmartTarget(current);
				current.ResumeValidate();
			}
			DataMonoBehaviour.processIndex += 1;
			if(canceled || index+1 > sorting.Length-1){
				EditorUtility.ClearProgressBar();
				Events.Remove("On Editor Update",DataMonoBehaviour.SortSmartNext);
				Events.Resume("On Hierarchy Changed");
			}
		}
		//===============
		// Shared
		//===============
		public static void SortSmartTarget(GameObject target){
			Component[] components = target.GetComponents<Component>().ToList().OrderBy(x=>x.GetAlias()).ToArray();
			DataMonoBehaviour.Sort(components);
			var controller = components.Find(x=>x is StateTable);
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
			Utility.ApplyPrefab(target);
			target.ResumeValidate();
			Events.Resume("On Hierarchy Changed");
		}
		//===============
		// Context
		//===============
		[ContextMenu("Sort (By Type)")]
		public void SortByType(){
			Component[] components = this.GetComponents<Component>().ToList().OrderBy(x=>x.GetType().Name).ToArray();
			DataMonoBehaviour.Sort(components);
		}
		[ContextMenu("Sort (By Alias)")]
		public void SortByAlias(){
			Component[] components = this.GetComponents<Component>().ToList().OrderBy(x=>x.GetAlias()).ToArray();
			DataMonoBehaviour.Sort(components);
		}
		[ContextMenu("Sort (Smart)")]
		public void SortSmart(){DataMonoBehaviour.SortSmartTarget(this.gameObject);}
		[ContextMenu("Move Element Up")]
		public void MoveItemUp(){this.MoveUp();}
		[ContextMenu("Move Element Down")]
		public void MoveItemDown(){this.MoveDown();}
		[ContextMenu("Move To Bottom")]
		public void MoveBottom(){this.MoveToBottom();}
		[ContextMenu("Move To Top")]
		public void MoveTop(){this.MoveToTop();}
		[ContextMenu("Apply Prefab")]
		public void ApplyPrefab(){DataMonoBehaviour.ApplyPrefabTarget(this.gameObject);}
		#endif
	}
	public class DataDependency{
		public bool exists;
		public bool processing;
		public string scriptName;
		public AttributeGameObject dynamicTarget;
		public GameObject target;
		public Type[] types;
		public string message;
		public Method method = ()=>{};
	}
}