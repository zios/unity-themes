using UnityEngine;
using UnityEditor;
using Zios;
using System.Collections;
using System.Collections.Generic;
[CustomEditor(typeof(ActionController))]
public class ActionControllerEditor : StateControllerEditor{
	public void AdjustPriority(object target,int amount){
		StateRow row = (StateRow)target;
		ActionController controller = (ActionController)this.target;
		controller.Refresh();
		int index = controller.table.IndexOf(row);
		if(index == 0 || index == controller.table.Length){
			return;
		}
		int adjacentPriority = 0;
		StateRow adjacentRow = controller.table[index+amount];
		adjacentPriority = ((ActionPart)adjacentRow.target).priority;
		ActionPart part = (ActionPart)row.target;
		part.priority = adjacentPriority + amount;
		controller.UpdateOrder();
		EditorUtility.SetDirty(part);
		this.BuildTable(true);
	}
	public override void MoveItemUp(object target){
		this.AdjustPriority(target,-1);
	}
	public override void MoveItemDown(object target){
		this.AdjustPriority(target,1);

	}
}