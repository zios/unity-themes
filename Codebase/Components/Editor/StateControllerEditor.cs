using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomEditor(typeof(StateController))]
public class StateControllerEditor : Editor{
	private CustomTableElement tableElement;
	public override void OnInspectorGUI(){
		if(this.tableElement == null){
			this.tableElement = new CustomTableElement(this.target);
		}
		this.tableElement.Draw();
		if(this.tableElement.shouldRepaint){
			this.Repaint();
		}
		if(GUI.changed){
			EditorUtility.SetDirty(this.target);
		}
	}
	public class CustomTableRow : TableRow{
		public CustomTableRow(string label,bool allowNegative,object target):base(label,allowNegative,target){}
		public override void PopulateChecks(){
			var stateRow = (StateRow<StateInterface,StateRequirement<StateInterface>>)this.target;
			foreach(StateRequirement<StateInterface> requirement in stateRow.requirements){
				if(requirement.requireOn){
					this.positiveChecks.Add(requirement.name);
				}
				else if(requirement.requireOff){
					this.negativeChecks.Add(requirement.name);
				}
			}
		}
		public override void Toggle(string state){
			var stateRow = (StateRow<StateInterface,StateRequirement<StateInterface>>)this.target;
			foreach(StateRequirement<StateInterface> requirement in stateRow.requirements){
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
			var stateRow = (StateRow<StateInterface,StateRequirement<StateInterface>>)this.target;
			string label = "End if unusable";
			if(stateRow.endIfUnusable){
				label += " ✓";
			}
			GUIContent field = new GUIContent(label);
			menu.AddItem(field,false,new GenericMenu.MenuFunction(ChangeEndIfUnusable));
			menu.ShowAsContext();
			Event.current.Use();
		}
		public void ChangeEndIfUnusable(){
			var stateRow = (StateRow<StateInterface,StateRequirement<StateInterface>>)this.target;
			stateRow.endIfUnusable = stateRow.endIfUnusable == false;
		}
	}
	public class CustomTableElement : TableTemplate{
		public CustomTableElement(UnityEngine.Object target):base(target){}
		public override void CreateHeaders(){
			this.headers.Add(string.Empty);
			foreach(var row in ((StateController)this.target).table.data){
				this.headers.Add(row.name);
				if(row.name.Length > this.labelSize){
					this.labelSize = row.name.Length;
				}
			}
		}
		public override void CreateItems(){
			this.tableItems = new List<TableRow>();
			foreach(var stateRow in ((StateController)this.target).table.data){
				this.tableItems.Add(new CustomTableRow(stateRow.name,true,stateRow));
			}
		}
	}
}