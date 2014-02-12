using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Zios.Editor;
[CustomEditor(typeof(StateController))]
public class StateControllerEditor : Editor{
	private TableGUI table = new TableGUI();
	public void OnEnable(){
		this.BuildTable();
	}
	public override void OnInspectorGUI(){
		this.table.Draw(); 
		if(GUI.changed){
			EditorUtility.SetDirty(target);
		}
	}
	public void BuildTable(bool force=false){
		StateController stateController = (StateController)this.target;
		if(force || (stateController != null && stateController.table != null)){
			this.table = new TableGUI();
			this.table.AddHeader("");
			foreach(StateRequirement requirement in stateController.table[0].requirements){
				this.table.AddHeader(requirement.name,null,this.OnClickHeader);
			}
			foreach(StateRow stateRow in stateController.table){
				TableRow tableRow = this.table.AddRow();
				tableRow.AddField(stateRow,null,this.OnClickRowLabel);
				foreach(StateRequirement requirement in stateRow.requirements){
					tableRow.AddField(requirement,this.OnDisplayField,this.OnClickField);
				}
			}
		}
	}
	public void OnDisplayField(TableField field){
		string value = "";
		StateRequirement requirement = (StateRequirement)field.target;
		GUIStyle style = GUI.skin.button;
		if(requirement.requireOn){
			value = "✓";
			style = GUI.skin.GetStyle("buttonOn");
		}
		else if(requirement.requireOff){
			value = "x";
			style = GUI.skin.GetStyle("buttonOff");
		}
		if(GUILayout.Button(new GUIContent(value),style)){
			field.onClick(field);
		}
		field.CheckClick();
	}
	public void OnClickHeader(TableHeaderItem header){}
	public void OnClickField(TableField field){
		int state = 0;
		StateRequirement requirement = (StateRequirement)field.target;
		if(requirement.requireOn){state = 1;}
		if(requirement.requireOff){state = 2;}
		state += Event.current.button == 0 ? 1 : -1;
		state = state.Modulus(3);
		requirement.requireOn = false;
		requirement.requireOff = false;
		if(state == 1){requirement.requireOn = true;}
		if(state == 2){requirement.requireOff = true;}
	}
	public void OnClickRowLabel(TableField field){
		if(Event.current.button == 0){}
		if(Event.current.button == 1){
			GenericMenu menu = new GenericMenu();
			StateRow stateRow = (StateRow)field.target;
			string label = "End if unusable";
			if(stateRow.endIfUnusable){
				label = "✓ " + label;
			}
			GUIContent endIfUnusableField = new GUIContent(label);
			GUIContent moveUp = new GUIContent("↑ Move Up");
			GUIContent moveDown = new GUIContent("↓ Move Down");
			menu.AddItem(moveUp,false,new GenericMenu.MenuFunction2(this.MoveItemUp),stateRow);
			menu.AddItem(endIfUnusableField,false,new GenericMenu.MenuFunction2(this.ChangeEndIfUnusable),stateRow);
			menu.AddItem(moveDown,false,new GenericMenu.MenuFunction2(this.MoveItemDown),stateRow);
			menu.ShowAsContext();
		}
		Event.current.Use();
	}
	public void MoveItem(int amount,object target){
		StateRow row = (StateRow)target;
		List<StateRow> table = new List<StateRow>(row.controller.table);
		int index = table.IndexOf(row);
		if(index == 0 && amount < 0){return;}
		if(index > table.Count - 2 && amount > 0){return;}
		table.Move(index,index + amount);
		row.controller.table = table.ToArray();
		EditorUtility.SetDirty(row.controller);
		this.BuildTable(true);
	}
	public void MoveItemUp(object target){this.MoveItem(-1,target);}
	public void MoveItemDown(object target){this.MoveItem(1,target);}
	public void ChangeEndIfUnusable(object target){
		StateRow row = (StateRow)target;
		row.endIfUnusable = !row.endIfUnusable;
		EditorUtility.SetDirty(row.controller);
	}
}