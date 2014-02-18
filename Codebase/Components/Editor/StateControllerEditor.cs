using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Zios.Editor;
[CustomEditor(typeof(StateController))]
public class StateControllerEditor : Editor{
	private Transform autoSelect;
	private TableGUI table = new TableGUI();
	public void OnEnable(){
		this.BuildTable(true);
	}
	public void OnDisable(){
		if(this.autoSelect != null){
			Selection.activeTransform = this.autoSelect;
			this.autoSelect = null;
		}
	}
	public override void OnInspectorGUI(){
		/*PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(((StateController)this.target));
		if(modifications.Length > 0){
			EditorGUILayout.HelpBox("Prefab changes must be applied for accurate table.",MessageType.Warning);
		}*/
		this.table.Draw(); 
		EditorUtility.SetDirty(this.target);
	}
	public void BuildTable(bool verticalHeader=false, bool force=false){
		StateController stateController = (StateController)this.target;
		if(force || (stateController != null && stateController.table != null)){
			this.table = new TableGUI();
			this.table.verticalHeader = verticalHeader;
			this.table.AddHeader("");
			foreach(StateRequirement requirement in stateController.table[0].requirements){
				this.table.AddHeader(requirement.name,null,this.OnClickHeader);
			}
			foreach(StateRow stateRow in stateController.table){
				TableRow tableRow = this.table.AddRow();
				tableRow.AddField(stateRow,this.OnDisplayRowLabel,this.OnClickRowLabel);
				foreach(StateRequirement requirement in stateRow.requirements){
					tableRow.AddField(requirement,this.OnDisplayField,this.OnClickField);
				}
			}
		}
	}
	public void OnDisplayRowLabel(TableField field){
		StateController controller = (StateController)this.target;
		StateRow row = (StateRow)field.target;
		GUIStyle style = GUI.skin.label;
		GUIContent content = new GUIContent(row.name,(string)row.id);
		if(row.target != null){
			if(row.target.usable){
				style = new GUIStyle(style);
				style.normal.textColor = Colors.Get("Silver");
			}
			if(row.target.inUse){
				style = new GUIStyle(style);
				style.normal.textColor = Colors.Get("BoldOrange");
			}
			if(controller.duplicates.ContainsKey(row.id)){
				style = new GUIStyle(style);
				style.normal.textColor = Colors.Get("BoldRed");
				string matches = "";
				foreach(StateInterface script in controller.duplicates[row.id]){
					matches += script.alias + ", ";
				}
				matches = matches.TrimRight(", ");
				content.text = "(!) " + content.text;
				content.tooltip = "Duplicate ID (" + matches + ").  Click to generate new.";
			}
		}
		if(field.selected){
			style = new GUIStyle(style);
			style.normal = style.active;	
		}
		GUILayout.Label(content,style);
		field.CheckClick();
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
			value = "X";
			style = GUI.skin.GetStyle("buttonOff");
		}
		if(field.selected){
			style = new GUIStyle(style);
			style.normal = style.active;	
		}
		if(GUILayout.Button(new GUIContent(value),style)){
			field.onClick(field);
		}
	}
	public void OnClickHeader(TableHeaderItem header){}
	public void OnClickField(TableField field){
		//field.selected = true;
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
		//field.selected = true;
		StateController controller = (StateController)this.target;
		StateRow stateRow = (StateRow)field.target;
		if(Event.current.button == 0){
			if(controller.duplicates.ContainsKey(stateRow.id)){
				string question = "Are you sure you wish to generate a new GUID for the " + stateRow.name + " element?";
				if(EditorUtility.DisplayDialog("Generate ID",question,"Yes","No")){
					controller.RepairRow(stateRow);
					controller.Awake();
					EditorUtility.SetDirty(controller);
					EditorUtility.SetDirty((MonoBehaviour)(stateRow.target));
					EditorUtility.SetDirty(((MonoBehaviour)this.target).gameObject);
					this.autoSelect = Selection.activeTransform;
					Selection.activeTransform = null;
				}
			}
		}
		if(Event.current.button == 1){
			GenericMenu menu = new GenericMenu();
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