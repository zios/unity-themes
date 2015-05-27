using Zios;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
    [ExecuteInEditMode]
    public class DataMonoBehaviour : MonoBehaviour{
	    public static GameObject[] sorting;
	    public static int processIndex;
	    public string alias;
		private string lastAlias;
	    [NonSerialized] public List<DataDependency> dependents = new List<DataDependency>();
	    public virtual void Awake(){
		    string name = this.GetType().Name.ToTitle();
		    this.lastAlias = this.alias = this.alias.SetDefault(name);
			this.dependents = new List<DataDependency>();
			if(!Application.isPlaying){
				Events.Register("On Destroy",this);
				Events.Register("On Validate",this);
				Events.Add("On Validate",this.CheckAlias,this);
				Events.Add("On Validate",this.CheckDependents,this);
				Events.Add("On Attributes Ready",this.CheckDependents);
			}
	    }
		//===============
		// Editor
		//===============
	    public virtual void OnValidate(){
			if(Application.isPlaying || Application.isLoadingLevel || !this.gameObject.activeInHierarchy){return;}
			this.CallEvent("On Validate");
	    }
	    public virtual void OnDestroy(){
			if(Application.isPlaying || Application.isLoadingLevel){return;}
			this.CallEvent("On Destroy");
			AttributeManager.PerformRefresh();
	    }
		public void CheckAlias(){
			if(this.lastAlias != this.alias || this.alias.IsEmpty()){
				AttributeManager.PerformRefresh();
				this.lastAlias = this.alias;
				this.Awake();
			}
		}
		public void CheckDependents(){
			foreach(var dependent in this.dependents){
				dependent.exists = false;
				if(dependent.type.IsNull()){continue;}
				if(dependent.target.IsNull() && dependent.dynamicTarget.IsNull()){continue;}
				if(dependent.target.IsNull() && !dependent.dynamicTarget.HasData()){continue;}
				GameObject target = dependent.target.IsNull() ? dependent.dynamicTarget.Get() : dependent.target;
				dependent.method = ()=>{};
				if(!target.IsNull()){
					Type type = dependent.type;
					dependent.exists = !target.GetComponent(type).IsNull();
					dependent.method = ()=>target.AddComponent(type);
				}
			}
		}
	    public void AddDependent<Type>() where Type : Component{this.AddDependent<Type>(this.gameObject,true);}
	    public void AddDependent<Type>(object target,bool isScript=false) where Type : Component{
			Method delayAdd = ()=>this.DelayAddDependent(typeof(Type),target,isScript);
			Events.AddLimited("On Attributes Ready",delayAdd,1);
	    }
	    public void DelayAddDependent(Type type,object target,bool isScript=false){
			if(this.dependents.Exists(x=>x.type==type)){return;}
			if(target.IsNull()){return;}
			var dependent = new DataDependency();
			dependent.dynamicTarget = target is AttributeGameObject ? (AttributeGameObject)target : null;
			dependent.target = target is AttributeGameObject ? null : (GameObject)target;
			dependent.type = type;
			dependent.scriptName = isScript ? this.GetType().Name : "";
			dependent.message = "[target] is missing required component : [type]. Click here to add.";
			this.dependents.AddNew(dependent);
			this.CheckDependents();
	    }
		//===============
		// Sorting	
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
		    var current = DataMonoBehaviour.sorting[index];
		    float total = (float)index/sorting.Length;
		    string message = index + " / " + sorting.Length + " -- " + current.GetPath();
		    bool canceled = EditorUtility.DisplayCancelableProgressBar("Sorting All Components",message,total);
			current.PauseValidate();
			DataMonoBehaviour.SortSmartTarget(current);
			current.ResumeValidate();
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
			var stateLink = components.Find(x=>x is StateLink);
			var controller = components.Find(x=>x is StateTable);
			if(!stateLink.IsNull()){stateLink.MoveToTop();}
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
		public string scriptName;
		public AttributeGameObject dynamicTarget;
		public GameObject target;
		public Type type;
		public string message;
		public Method method = ()=>{};
	}
}
