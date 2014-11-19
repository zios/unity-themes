using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Zios.Editor;
[CustomEditor(typeof(StateController))]
public class StateControllerEditor : Editor{
	public float nextStep;
	private Transform autoSelect;
	private TableGUI tableGUI = new TableGUI();
	private StateRow[] data;
	private int tableIndex = 0;
	private Dictionary<StateRow,int> rowIndex = new Dictionary<StateRow,int>();
	public virtual void OnEnable(){
		StateController stateController = (StateController)this.target;
		stateController.UpdateTableList();
		this.BuildTable(true);
	}
	public virtual void OnDisable(){
		if(this.autoSelect != null){
			Selection.activeTransform = this.autoSelect;
			this.autoSelect = null;
		}
	}
	public void EditorUpdate(){
		StateController stateController = (StateController)this.target;
		if(!Application.isPlaying && Time.realtimeSinceStartup > this.nextStep){
			this.nextStep = Time.realtimeSinceStartup + 1f;
			stateController.Refresh();
		}
	}
	public override void OnInspectorGUI(){
		Utility.EditorCall(this.EditorUpdate);
		StateController stateController = (StateController)this.target;
		stateController.UpdateTableList();
		StateRow[] activeTable = stateController.tables[this.tableIndex];
		if(this.data != activeTable){
			this.data = activeTable;
			this.BuildTable(true);
		}
		if(!stateController.advanced){
			this.tableIndex = 0;
		}
		if(activeTable.Length > 0){
			//string tableName = ((MonoBehaviour)activeTable[0].target).name;
			this.tableGUI.tableSkin.label.fixedWidth = 0;
			foreach(StateRow stateRow in activeTable){
				int size = (int)(GUI.skin.label.CalcSize(new GUIContent(stateRow.name)).x) + 24;
				size = (size / 8) * 8 + 1;
				if(size > this.tableGUI.tableSkin.label.fixedWidth){
					this.tableGUI.tableSkin.label.fixedWidth = size;
				}	
			}
		}
		this.tableGUI.Draw();
		this.DrawControls();
		EditorUtility.SetDirty(this.target);
	}
	public virtual void DrawControls(){
		StateController stateController = (StateController)this.target;
		string useStyle = "titleTabDisabled";
		string endStyle = "titleTabDisabled";
		if(stateController.advanced){
			useStyle = this.tableIndex == 0 ? "titleTabGreen" : "titleTabInactive";
			endStyle = this.tableIndex == 1 ? "titleTabRed" : "titleTabInactive";
		}
		//EditorGUI.LabelField(area,"",GUI.skin.GetStyle("title"));
		Rect area = EditorGUILayout.GetControlRect();
		area = new Rect(area.x+20,area.y+10,16,16);
		stateController.advanced = EditorGUI.ToggleLeft(area,"",stateController.advanced);
		area.x += 20;
		area = new Rect(area.x,area.y,80,32);
		if(GUI.Button(area,"Use",GUI.skin.GetStyle(useStyle)) && stateController.advanced){
			this.tableIndex = 0;
		}
		area.x += 82;
		if(GUI.Button(area,"End",GUI.skin.GetStyle(endStyle)) && stateController.advanced){
			this.tableIndex = 1;
		}
		GUILayout.Space(25);
	}
	public virtual void BuildTable(bool verticalHeader=false,bool force=false){
		StateController stateController = (StateController)this.target;
		StateRow[] activeTable = stateController.tables[this.tableIndex];
		if(force || (stateController != null && activeTable != null)){
			this.tableGUI = new TableGUI();
			this.tableGUI.SetHeader(verticalHeader,true,this.CompareRows);
			this.tableGUI.AddHeader("","");
			if(activeTable.Length > 0){
				StateRequirement[] firstRow = activeTable[0].requirements[0].data;
				foreach(StateRequirement requirement in firstRow){
					//MonoBehaviour requireScript = requirement.target as MonoBehaviour;
					//if(requireScript != null && !requireScript.gameObject.activeInHierarchy){continue;}
					this.tableGUI.AddHeader(requirement.name,"",null,this.OnClickHeader);
				}
				foreach(StateRow stateRow in activeTable){
					//MonoBehaviour rowScript = stateRow.target as MonoBehaviour;
					//if(rowScript != null && !rowScript.gameObject.activeInHierarchy){continue;}
					if(!this.rowIndex.ContainsKey(stateRow)){
						this.rowIndex[stateRow] = 0;
					}
					int rowIndex = this.rowIndex[stateRow];
					TableRow tableRow = this.tableGUI.AddRow(stateRow);
					tableRow.AddField(stateRow,this.OnDisplayRowLabel,this.OnClickRowLabel);
					//List<StateRequirement> sorted = stateRow.requirements[rowIndex].data.OrderBy(a=>a.name).ToList();
					foreach(StateRequirement requirement in stateRow.requirements[rowIndex].data){
						//MonoBehaviour requirementScript = requirement.target as MonoBehaviour;
						//if(requirementScript != null && !requirementScript.gameObject.activeInHierarchy){continue;}
						tableRow.AddField(requirement,this.OnDisplayField,this.OnClickField);
					}
				}
			}
		}
	}
	public virtual void OnDisplayRowLabel(TableField field){
		StateRow row = (StateRow)field.target;
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.normal.textColor = Colors.Get("Gray");
		GUIContent content = new GUIContent(row.name,(string)row.id);
		if(row.target != null){
			if(row.target.usable){
				style.normal.textColor = EditorGUIUtility.isProSkin ? Colors.Get("Silver") : Colors.Get("DarkGray");
			}
			if(row.target.inUse){
				style.normal.textColor = EditorGUIUtility.isProSkin ? Colors.Get("BoldOrange") : Colors.Get("DarkBlue");
			}
		}
		if(field.selected){
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