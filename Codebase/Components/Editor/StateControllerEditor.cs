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
		public CustomTableRow(string label,bool allowNegative,object target):base(label,allowNegative,target){
		}
		public override void PopulateChecks(){
			StateTable stateTable = (StateTable)this.target;
			foreach(StateRequirement requirement in stateTable.requirements){
				if(requirement.requireOn){
					this.positiveChecks.Add(requirement.name);
				}
				else if(requirement.requireOff){
					this.negativeChecks.Add(requirement.name);
				}
			}
		}
		public override void Toogle(string state){
			StateTable stateTable = (StateTable)this.target;
			foreach(StateRequirement requirement in stateTable.requirements){
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
			StateTable stateTable = (StateTable)this.target;
			string label = "End if unusable";
			if(stateTable.endIfUnusable){
				label += " ✓";
			}
			GUIContent field = new GUIContent(label);
			menu.AddItem(field,false,new GenericMenu.MenuFunction(ChangeEndIfUnusable));
			menu.ShowAsContext();
			Event.current.Use();
		}
		public void ChangeEndIfUnusable(){
			StateTable stateTable = (StateTable)this.target;
			stateTable.endIfUnusable = stateTable.endIfUnusable == false;
		}
	}
	public class CustomTableElement : TableTemplate{
		public CustomTableElement(UnityEngine.Object target):base(target){
		}
		public override void CreateHeaders(){
			this.headers.Add(string.Empty);
			foreach(StateTable table in ((StateController)target).data){
				this.headers.Add(table.name);
				if(table.name.Length > this.labelSize){
					this.labelSize = table.name.Length;
				}
			}
		}
		public override void CreateItems(){
			this.tableItems = new List<TableRow>();
			foreach(StateTable stateTable in ((StateController)target).data){
				this.tableItems.Add(new CustomTableRow(stateTable.name,true,stateTable));
			}
		}
	}
}