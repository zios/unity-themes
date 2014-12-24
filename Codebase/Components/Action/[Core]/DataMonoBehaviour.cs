using Zios;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Action = Zios.Action;
[Serializable][AddComponentMenu("")]
public class DataMonoBehaviour : MonoBehaviour{
	public string alias;
	public virtual void OnApplicationQuit(){this.Awake();}
	public virtual void Reset(){this.Awake();}
	public virtual void Awake(){
		string name = this.GetType().ToString().ToTitle();
		this.alias = this.alias.SetDefault(name);
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
		var action = components.Find(x=>x is Action);
		var controller = components.Find(x=>x is StateController);
		if(!action.IsNull()){action.MoveToTop();}
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
}