using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Zios.Editor;
[CustomEditor(typeof(StateController))]
public class StateControllerEditor : Editor{
	private TableGUI table = new TableGUI();
	public void OnEnable(){
		StateController stateController = (StateController)this.target;
		this.table.AddHeader("");
		this.table.rows = new List<TableRow>();
		if(stateController != null && stateController.table != null){
			foreach(StateRow stateRow in stateController.table){
				this.table.AddHeader(stateRow.name);
				TableRow tableRow = this.table.AddRow();
				tableRow.AddField(stateRow.name,stateRow,TableFieldType.String,this.CheckContext);
				foreach(StateRequirement requirement in stateRow.requirements){
					tableRow.AddField(requirement,this.OnDisplay,this.OnClick);
				}
			}
		}
	}
	public override void OnInspectorGUI(){
		this.table.Draw(); 
		if(GUI.changed){
			EditorUtility.SetDirty(target);
		}
	}
	public void OnDisplay(TableField field){
		field.value = "";
		StateRequirement requirement = (StateRequirement)field.target;
		if(requirement.requireOn){
			field.value = "✓";
		}
		else if(requirement.requireOff){
			field.value = "x";
		}
		GUIStyle style = GUI.skin.button;
		if(GUILayout.Button(new GUIContent(field.value),style)){
			field.onClick(field);
		}
	}
	public void OnClick(TableField field){
		StateRequirement requirement = (StateRequirement)field.target;
		if(requirement.requireOn){
			requirement.requireOn = false;
			requirement.requireOff = true;
		}
		else if(requirement.requireOff){
			requirement.requireOn = false;
			requirement.requireOff = false;
		}
		else{
			requirement.requireOn = true;
			requirement.requireOff = false;
		}
		this.OnDisplay(field);
	}
	public void CheckContext(TableField field){
		GenericMenu menu = new GenericMenu();
		StateRow stateRow = (StateRow)field.target;
		string label = "End if unusable";
		if(stateRow.endIfUnusable){
			label += " ✓";
		}
		GUIContent endIfUnusableField = new GUIContent(label);
		GUIContent moveUp = new GUIContent("↑ Move Up");
		GUIContent moveDown = new GUIContent("↓ Move Down");
		menu.AddItem(moveUp,false,new GenericMenu.MenuFunction2(this.MoveItemUp),stateRow);
		menu.AddItem(endIfUnusableField,false,new GenericMenu.MenuFunction2(this.ChangeEndIfUnusable),stateRow);
		menu.AddItem(moveDown,false,new GenericMenu.MenuFunction2(this.MoveItemDown),stateRow);
		menu.ShowAsContext();
		Event.current.Use();
	}
	public void MoveItem(int amount,StateRow row){
		List<StateRow> table = new List<StateRow>(row.controller.table);
		int index = table.IndexOf(row);
		if(index == 0 && amount < 0){
			return;
		}
		if(index > table.Count - 2 && amount > 0){
			return;
		}
		//table.Move(index,index + amount);
		row.controller.table = table.ToArray();			
	}
	public void MoveItemUp(object stateRow){
		this.MoveItem(-1,(StateRow)stateRow);
	}
	public void MoveItemDown(object stateRow){
		this.MoveItem(1,(StateRow)stateRow);
	}
	public void ChangeEndIfUnusable(object row){
		StateRow stateRow = (StateRow)row;
		stateRow.endIfUnusable = stateRow.endIfUnusable == false;
	}
}