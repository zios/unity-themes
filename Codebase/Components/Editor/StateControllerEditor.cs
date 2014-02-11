using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomEditor(typeof(StateController))]
public class StateControllerEditor : Editor{
	private CustomTableElement tableElement;
	public override void OnInspectorGUI(){
		if(this.tableElement == null){
			this.tableElement = new CustomTableElement(target);
		}
		this.tableElement.Draw();
		if(this.tableElement.shouldRepaint){
			this.Repaint();
		}
		if(GUI.changed){
			EditorUtility.SetDirty(target);
		}
	}
	public class CustomTableRow : TableRow{
		public CustomTableRow(string label,bool allowNegative,object target):base(label,allowNegative,target){}
		public override void PopulateChecks(){
			StateRow StateRow = (StateRow)this.target;
			foreach(StateRequirement requirement in StateRow.requirements){
				if(requirement.requireOn){
					this.positiveChecks.Add(requirement.name);
				}
				else if(requirement.requireOff){
					this.negativeChecks.Add(requirement.name);
				}
			}
		}
		public override void Toggle(string state){
			StateRow StateRow = (StateRow)this.target;
			foreach(StateRequirement requirement in StateRow.requirements){
				if(requirement.name.Equals(state)){
					if(requirement.requireOn){
						requirement.requireOn = false;
						if(this.allowNegative){
							requirement.requireOff = true;
						}
					}
					else if(requirement.requireOff){
						requirement.requireOff = false;
					}
					else{
						requirement.requireOn = true;
						requirement.requireOff = false;
					}
				}
			}
		}
		public override void CheckContext(){
			GenericMenu menu = new GenericMenu();
			StateRow StateRow = (StateRow)this.target;
			string label = "End if unusable";
			if(StateRow.endIfUnusable){
				label += " ✓";
			}
			GUIContent field = new GUIContent(label);
			GUIContent moveUp = new GUIContent("↑ Move Up");
			GUIContent moveDown = new GUIContent("↓ Move Down");
			menu.AddItem(moveUp,false,new GenericMenu.MenuFunction(this.MoveItemUp));
			menu.AddItem(field,false,new GenericMenu.MenuFunction(this.ChangeEndIfUnusable));
			menu.AddItem(moveDown,false,new GenericMenu.MenuFunction(this.MoveItemDown));
			menu.ShowAsContext();
			Event.current.Use();
		}
		public void MoveItem(int amount){
			StateRow row = (StateRow)this.target;
			List<StateRow> table = new List<StateRow>(row.controller.table);
			int index = table.IndexOf(row);
			if(index == 0 && amount < 0){return;}
			if(index > table.Count-2 && amount > 0){return;}
			table.Move(index,index+amount);
			row.controller.table = table.ToArray();			
		}
		public void MoveItemUp(){this.MoveItem(-1);}
		public void MoveItemDown(){this.MoveItem(1);}
		public void ChangeEndIfUnusable(){
			StateRow StateRow = (StateRow)this.target;
			StateRow.endIfUnusable = StateRow.endIfUnusable == false;
		}
	}
	public class CustomTableElement : TableTemplate{
		public CustomTableElement(UnityEngine.Object target):base(target){}
		public override void CreateHeaders(){
			this.headers.Add(string.Empty);
			this.labelSize = 125;
			foreach(StateRow row in ((StateController)target).table){
				foreach(StateRequirement requirement in row.requirements){
					this.headers.Add(requirement.name);
				}
				break;
			}
		}
		public override void CreateItems(){
			this.tableItems = new List<TableRow>();
			foreach(StateRow row in ((StateController)target).table){
				this.tableItems.Add(new CustomTableRow(row.name,true,row));
			}
		}
	}
}