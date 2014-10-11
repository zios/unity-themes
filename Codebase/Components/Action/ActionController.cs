using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Action = Zios.Action;
using ActionPart = Zios.ActionPart;
[RequireComponent(typeof(Action))]
[ExecuteInEditMode][AddComponentMenu("Zios/Component/Action/Action Controller")]
public class ActionController : StateController{
	private Action action;
	private ActionPart[] parts = new ActionPart[0];
	public override void Reset(){
		this.action = null;
		base.Reset();
	}
	public override void Awake(){
		Events.Add("Refresh",this.Refresh);
		Events.Add("UpdateParts",this.UpdateStates);
		if(this.action == null){
			this.action = this.GetComponent<Action>();
			this.Refresh();
		}
	}
	[ContextMenu("Refresh")]
	public override void Refresh(){
		this.UpdateScripts("ActionPart");
		this.UpdateRows();
		this.UpdateRequirements();
		this.UpdateOrder();
		//this.UpdatePriorityOrder();
	}
	public override void Update(){
		if(Application.isEditor){
			ActionPart[] parts = this.gameObject.GetComponents<ActionPart>();
			int total = parts.Length;
			if(this.total != total){
				this.total = total;
				foreach(ActionPart part in parts){
					if(!this.parts.Contains(part)){
						part.OnValidate();
					}
				}
				this.Refresh();
				this.parts = parts;
			}
		}
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