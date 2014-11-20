#pragma warning disable 0414
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Action = Zios.Action;
using ActionPart = Zios.ActionPart;
[AddComponentMenu("Zios/Component/Action/*/Action Controller")]
public class ActionController : StateController{
	private Action action;
	public override void Reset(){
		this.action = null;
		base.Reset();
	}
	public override void Awake(){
		this.action = this.GetComponent<Action>();
		Events.Add("@Update Parts",this.UpdateStates);
		Events.Add("@Refresh",this.Refresh);
		this.Refresh();
	}
	[ContextMenu("Refresh")]
	public override void Refresh(){
		this.UpdateTableList();
		this.UpdateScripts<ActionPart>(false);
		this.ResolveDuplicates();
		this.UpdateRows();
		this.UpdateRequirements();
		this.UpdateOrder();
		//this.UpdatePriorityOrder();
	}
	public void UpdatePriorityOrder(){
		Dictionary<StateRow,int> data = new Dictionary<StateRow,int>();	
		List<StateRow> result = new List<StateRow>();
		foreach(StateRow row in this.table){
			data[row] = ((ActionPart)row.target).priority;
		}
		foreach(var item in data.OrderBy(x=>x.Value)){
			result.Add(item.Key);
		}
		this.table = result.ToArray();
	}
}