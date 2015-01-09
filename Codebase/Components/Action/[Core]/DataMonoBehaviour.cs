using Zios;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
[Serializable][AddComponentMenu("")]
public class DataMonoBehaviour : MonoBehaviour{
	public static DataMonoBehaviour[] sorting;
	public static int processIndex;
	public string alias;
	public virtual void OnApplicationQuit(){this.Awake();}
	public virtual void Reset(){this.Awake();}
	public virtual void Awake(){
		string name = this.GetType().ToString().ToTitle();
		this.alias = this.alias.SetDefault(name);
	}
	#if UNITY_EDITOR
    [MenuItem("Zios/Process/Components/Sort All (Smart)")]
	public static void SortSmartAll(){
		var unique = new List<DataMonoBehaviour>();
		DataMonoBehaviour.sorting = Locate.GetSceneComponents<DataMonoBehaviour>();
		foreach(var behaviour in DataMonoBehaviour.sorting){
			if(behaviour.IsNull() || behaviour.gameObject.IsNull()){continue;}
			if(!unique.Exists(x=>x.gameObject==behaviour.gameObject)){
				unique.Add(behaviour);
			}
		}
		DataMonoBehaviour.sorting = unique.ToArray();
		DataMonoBehaviour.processIndex = 0;
		Utility.EditorUpdate(DataMonoBehaviour.SortSmartNext,true);
		Utility.PauseHierarchyUpdates();
	}
	public static void SortSmartNext(){
		int index = DataMonoBehaviour.processIndex;
		var sorting = DataMonoBehaviour.sorting;
		var current = DataMonoBehaviour.sorting[index];
		float total = (float)index/sorting.Length;
		string message = index + " / " + sorting.Length + " -- " + current.gameObject.name;
		bool canceled = EditorUtility.DisplayCancelableProgressBar("Sorting All Components",message,total);
		current.SortSmart();
		DataMonoBehaviour.processIndex += 1;
		if(canceled || index+1 > sorting.Length-1){
			Utility.RemoveEditorUpdate(DataMonoBehaviour.SortSmartNext);
			EditorUtility.ClearProgressBar();
			Utility.ResumeHierarchyUpdates();
		}
	}
	[ContextMenu("Sort (By Type)")]
	public void SortByType(){
		Component[] components = this.GetComponents<Component>().ToList().OrderBy(x=>x.GetType().Name).ToArray();
		this.Sort(components);
	}
	[ContextMenu("Sort (By Alias)")]
	public void SortByAlias(){
		Component[] components = this.GetComponents<Component>().ToList().OrderBy(x=>x.GetAlias()).ToArray();
		this.Sort(components);
	}
	[ContextMenu("Sort (Smart)")]
	public void SortSmart(){
		Component[] components = this.GetComponents<Component>().ToList().OrderBy(x=>x.GetAlias()).ToArray();
		this.Sort(components);
		var stateLink = components.Find(x=>x is StateLink);
		var controller = components.Find(x=>x is StateTable);
		if(!stateLink.IsNull()){stateLink.MoveToTop();}
		if(!controller.IsNull()){controller.MoveToTop();}
	}
	public void Sort(Component[] components){
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
	[ContextMenu("Move Element Up")]
	public void MoveItemUp(){this.MoveUp();}
	[ContextMenu("Move Element Down")]
	public void MoveItemDown(){this.MoveDown();}
	[ContextMenu("Move To Bottom")]
	public void MoveBottom(){this.MoveToBottom();}
	[ContextMenu("Move To Top")]
	public void MoveTop(){this.MoveToTop();}
	#endif
}
