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
		this.table.tableSkin.label.fixedWidth = 0;
		StateController stateController = (StateController)this.target;
		foreach(StateRow stateRow in stateController.table){
			int size = (stateRow.name.Length) * this.table.tableSkin.label.fontSize;
			if(size > this.table.tableSkin.label.fixedWidth){
				this.table.tableSkin.label.fixedWidth = size;
			}	
		}
		this.table.Draw(); 
		EditorUtility.SetDirty(this.target);
	}
	public void BuildTable(bool verticalHeader=false,bool force=false){
		StateController stateController = (StateController)this.target;
		if(force || (stateController != null && stateController.table != null)){
			this.table = new TableGUI();
			this.table.SetHeader(verticalHeader,true,this.CompareRows);
			this.table.AddHeader("","");
			foreach(StateRequirement requirement in stateController.table[0].requirements){
				this.table.AddHeader(requirement.name,"",null,this.OnClickHeader);
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
		field.empty = true;
		StateRequirement requirement = (StateRequirement)field.target;
		GUIStyle style = GUI.skin.button;
		if(requirement.requireOn){
			value = requirement.index == 0 ? "✓" : requirement.index.ToString();
			field.empty = false;
			style = GUI.skin.GetStyle("buttonOn");
			style.padding.left = requirement.index == 0 ? 6 : 8;
		}
		else if(requirement.requireOff){
			value = requirement.index == 0 ? "X" : requirement.index.ToString();
			field.empty = false;
			style = GUI.skin.GetStyle("buttonOff");
			style.padding.left = requirement.index == 0 ? 8 : 8;
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
		int state = 0;
		StateRequirement requirement = (StateRequirement)field.target;
		int index = requirement.index;
		if(requirement.requireOn){
			state = 1;
		}
		if(requirement.requireOff){
			state = 2;
		}
		int amount = Event.current.button == 0 ? 1 : -1;
		state += amount;
		if(state == -1){index -= 1;}
		if(state == 3){index += 1;}
		requirement.index = Mathf.Clamp(index,0,9);
		state = state.Modulus(3);
		requirement.requireOn = false;
		requirement.requireOff = false;
		if(state == 1){
			requirement.requireOn = true;
		}
		if(state == 2){
			requirement.requireOff = true;
		}
	}
	public int CompareRows(object target1,object target2){
		if(target1 is StateRequirement && target2 is StateRequirement){
			StateRequirement requirement1 = (StateRequirement)target1;
			StateRequirement requirement2 = (StateRequirement)target2;
			if(requirement1.requireOn && !requirement2.requireOn){
				return -1;
			}
			if(!requirement1.requireOn && requirement2.requireOn){
				return 1;
			}
			if(!requirement1.requireOn && !requirement2.requireOn){
				if(requirement1.requireOff && !requirement2.requireOff){
					return -1;
				}
				if(!requirement1.requireOff && requirement2.requireOff){
					return 1;
				}
			}
		}
		return 0;
	}
	public void GenerateGUID(object row){
		StateRow stateRow = (StateRow)row;
		string question = "Are you sure you wish to generate a new GUID for the " + stateRow.name + " element?";
		if(EditorUtility.DisplayDialog("Generate ID",question,"Yes","No")){
			StateController controller = (StateController)stateRow.controller;
			controller.RepairRow(stateRow);
			controller.Awake();
			EditorUtility.SetDirty(controller);
			EditorUtility.SetDirty((MonoBehaviour)(stateRow.target));
			EditorUtility.SetDirty(((MonoBehaviour)controller).gameObject);
			this.autoSelect = Selection.activeTransform;
			Selection.activeTransform = null;
		}
	}
	public void OnClickRowLabel(TableField field){
		//field.selected = true;
		StateController controller = (StateController)this.target;
		StateRow stateRow = (StateRow)field.target;
		if(Event.current.button == 0){
			if(controller.duplicates.ContainsKey(stateRow.id)){
				this.GenerateGUID(stateRow);
			}
		}
		if(Event.current.button == 1){
			GenericMenu menu = new GenericMenu();
			string label = "Persist While Unusable";
			if(stateRow.persistWhileUnusable){
				label = "✓ " + label;
			}
			GUIContent persistWhileUnusableField = new GUIContent(label);
			GUIContent generateGUID = new GUIContent("Generate New GUID");
			GUIContent moveUp = new GUIContent("↑ Move Up");
			GUIContent moveDown = new GUIContent("↓ Move Down");
			menu.AddItem(moveUp,false,new GenericMenu.MenuFunction2(this.MoveItemUp),stateRow);
			menu.AddItem(persistWhileUnusableField,false,new GenericMenu.MenuFunction2(this.ChangePersistWhileUnusable),stateRow);
			menu.AddItem(generateGUID,false,new GenericMenu.MenuFunction2(this.GenerateGUID),stateRow);
			menu.AddItem(moveDown,false,new GenericMenu.MenuFunction2(this.MoveItemDown),stateRow);
			menu.ShowAsContext();
		}
		Event.current.Use();
	}
	public void MoveItem(int amount,object target){
		StateRow row = (StateRow)target;
		List<StateRow> table = new List<StateRow>(row.controller.table);
		int index = table.IndexOf(row);
		if(index == 0 && amount < 0){
			return;
		}
		if(index > table.Count - 2 && amount > 0){
			return;
		}
		table.Move(index,index + amount);
		row.controller.table = table.ToArray();
		EditorUtility.SetDirty(row.controller);
		this.BuildTable(true);
	}
	public void MoveItemUp(object target){
		this.MoveItem(-1,target);
	}
	public void MoveItemDown(object target){
		this.MoveItem(1,target);
	}
	public void ChangePersistWhileUnusable(object target){
		StateRow row = (StateRow)target;
		row.persistWhileUnusable = !row.persistWhileUnusable;
		EditorUtility.SetDirty(row.controller);
	}
}