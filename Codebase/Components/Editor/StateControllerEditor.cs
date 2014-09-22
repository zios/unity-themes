using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Zios.Editor;
[CustomEditor(typeof(StateController))]
public class StateControllerEditor : Editor{
	private Transform autoSelect;
	private TableGUI table = new TableGUI();
	private StateRow[] data;
	private Dictionary<StateRow,int> rowIndex = new Dictionary<StateRow,int>();
	public virtual void OnEnable(){
		this.BuildTable(true);
	}
	public virtual void OnDisable(){
		if(this.autoSelect != null){
			Selection.activeTransform = this.autoSelect;
			this.autoSelect = null;
		}
	}
	public override void OnInspectorGUI(){
		StateController stateController = (StateController)this.target;
		if(this.data != stateController.table){
			this.data = stateController.table;
			this.BuildTable(true);
		}
		this.table.tableSkin.label.fixedWidth = 0;
		foreach(StateRow stateRow in stateController.table){
			int size = (int)(GUI.skin.label.CalcSize(new GUIContent(stateRow.name)).x) + 24;
			size = (size / 8) * 8 + 1;
			if(size > this.table.tableSkin.label.fixedWidth){
				this.table.tableSkin.label.fixedWidth = size;
			}	
		}
		this.table.Draw(); 
		EditorUtility.SetDirty(this.target);
	}
	public virtual void BuildTable(bool verticalHeader=false,bool force=false){
		StateController stateController = (StateController)this.target;
		if(force || (stateController != null && stateController.table != null)){
			this.table = new TableGUI();
			this.table.SetHeader(verticalHeader,true,this.CompareRows);
			this.table.AddHeader("","");
			StateRequirement[] firstRow = stateController.table[0].requirements[0].data;
			foreach(StateRequirement requirement in firstRow){
				this.table.AddHeader(requirement.name,"",null,this.OnClickHeader);
			}
			foreach(StateRow stateRow in stateController.table){
				if(!this.rowIndex.ContainsKey(stateRow)){
					this.rowIndex[stateRow] = 0;
				}
				int rowIndex = this.rowIndex[stateRow];
				TableRow tableRow = this.table.AddRow(stateRow);
				tableRow.AddField(stateRow,this.OnDisplayRowLabel,this.OnClickRowLabel);
				//List<StateRequirement> sorted = stateRow.requirements[rowIndex].data.OrderBy(a=>a.name).ToList();
				foreach(StateRequirement requirement in stateRow.requirements[rowIndex].data){
					tableRow.AddField(requirement,this.OnDisplayField,this.OnClickField);
				}
			}
		}
	}
	public virtual void OnDisplayRowLabel(TableField field){
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
		}
		if(field.selected){
			style = new GUIStyle(style);
			style.normal = style.active;	
		}
		GUILayout.Label(content,style);
		field.CheckClick();
	}
	public virtual void OnDisplayField(TableField field){
		string value = "";
		field.empty = true;
		StateRow row = (StateRow)field.row.target;
		StateRequirement requirement = (StateRequirement)field.target;
		bool useIndexes = row != null && this.rowIndex[row] != 0;
		GUIStyle style = GUI.skin.button;
		if(requirement.requireOn){
			value = useIndexes ? this.rowIndex[row].ToString() : "✓";
			field.empty = false;
			style = GUI.skin.GetStyle("buttonOn");
			style.padding.left = useIndexes ? 8 : 6;
		}
		else if(requirement.requireOff){
			value = useIndexes ? this.rowIndex[row].ToString() : "X";
			field.empty = false;
			style = GUI.skin.GetStyle("buttonOff");
			style.padding.left = 8;
		}
		if(field.selected){
			style = new GUIStyle(style);
			style.normal = style.active;	
		}
		if(GUILayout.Button(new GUIContent(value),style)){
			field.onClick(field);
		}
	}
	public virtual void OnClickHeader(TableHeaderItem header){}
	public virtual void OnClickField(TableField field){
		int state = 0;
		StateRequirement requirement = (StateRequirement)field.target;
		if(requirement.requireOn){state = 1;}
		if(requirement.requireOff){state = 2;}
		int amount = Event.current.button == 0 ? 1 : -1;
		state += amount;
		state = state.Modulus(3);
		requirement.requireOn = false;
		requirement.requireOff = false;
		if(state == 1){requirement.requireOn = true;}
		if(state == 2){requirement.requireOff = true;}
	}
	public virtual int CompareRows(object target1,object target2){
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
	public virtual void OnClickRowLabel(TableField field){
		StateRow stateRow = (StateRow)field.target;
		if(Event.current.button == 0){
			int length = stateRow.requirements.Length;
			int index = this.rowIndex[stateRow];
			index += Event.current.control ? -1 : 1;
			if(index < 0){index = length-1;}
			if(index >= length){index = 0;}
			this.rowIndex[stateRow] = index;
			this.BuildTable(true);
		}
		if(Event.current.button == 1){
			GenericMenu menu = new GenericMenu();
			GUIContent addAlternative = new GUIContent("+ Add Alternate Row");
			GUIContent removeAlternative = new GUIContent("- Remove Alternative Row");
			//GUIContent moveUp = new GUIContent("↑ Move Up");
			//GUIContent moveDown = new GUIContent("↓ Move Down");
			//menu.AddItem(moveUp,false,new GenericMenu.MenuFunction2(this.MoveItemUp),stateRow);
			menu.AddItem(addAlternative,false,new GenericMenu.MenuFunction2(this.AddAlternativeRow),stateRow);
			if(this.rowIndex[stateRow] != 0){
				menu.AddItem(removeAlternative,false,new GenericMenu.MenuFunction2(this.RemoveAlternativeRow),stateRow);
			}
			//menu.AddItem(moveDown,false,new GenericMenu.MenuFunction2(this.MoveItemDown),stateRow);
			menu.ShowAsContext();
		}
		Event.current.Use();
	}
	public void AddAlternativeRow(object target){
		StateRow row = (StateRow)target;
		List<StateRowData> data = new List<StateRowData>(row.requirements);
		data.Add(new StateRowData());
		row.requirements = data.ToArray();
		((StateController)this.target).Refresh();
		this.rowIndex[row] = row.requirements.Length-1;
		this.BuildTable(true);
	}
	public void RemoveAlternativeRow(object target){
		StateRow row = (StateRow)target;
		int index = this.rowIndex[row];
		List<StateRowData> data = new List<StateRowData>(row.requirements);
		data.RemoveAt(index);
		row.requirements = data.ToArray();
		this.rowIndex[row] = index-1;
		this.BuildTable(true);
	}
	public virtual void MoveItem(int amount,object target){
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
	public virtual void MoveItemUp(object target){
		this.MoveItem(-1,target);
	}
	public virtual void MoveItemDown(object target){
		this.MoveItem(1,target);
	}
}