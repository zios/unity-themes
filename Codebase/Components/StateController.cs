using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
[Serializable]
public class StateTable{
	public string name;
	public bool endIfUnusable = false;
	public List<StateRequirement> requirements = new List<StateRequirement>();
	[HideInInspector] public Component self;
	public StateTable(string name,Component component){
		this.name = name;
		this.self = component;
	}
}
[Serializable]
public class StateRequirement{
	public string name;
	public bool requireOn;
	public bool requireOff;
	[HideInInspector] public Component target;
	public StateRequirement(string name,Component component){
		this.name = name;
		this.target = component;
	}
}
[ExecuteInEditMode][AddComponentMenu("Zios/Component/General/State Controller")]
public class StateController : MonoBehaviour{
	public List<Component> actions = new List<Component>();
	public List<StateTable> data = new List<StateTable>();
	public void Awake(){
		Events.Add("UpdateStates",this.UpdateStates);
		this.UpdateActions();
		this.UpdateTables();
		this.UpdateRequirements();
	}
	public void UpdateStates(){
		foreach(StateTable table in this.data){
			foreach(StateRequirement requirement in table.requirements){
				CoreAction action = (CoreAction)table.self;
				CoreAction target = (CoreAction)requirement.target;
				bool state = target.inUse;
				bool mismatchOn = requirement.requireOn && !state;
				bool mismatchOff = requirement.requireOff && state;
				action.usable = !(mismatchOn || mismatchOff);
				if(!action.usable){
					if(table.endIfUnusable){
						action.End();
					}
					break;
				}
			}
		}
	}
	public void UpdateActions(){
		Component[] all = this.gameObject.GetComponents<Component>();
		foreach(Component component in all){
			bool contains = this.actions.Contains(component);
			bool applicable = component.HasAttribute("usable") && component.HasAttribute("inUse");
			if(!contains && applicable){
				this.actions.Add(component);
			}
		}
		this.actions = this.actions.OrderBy(x=>x.name).ToList();
	}
	public void UpdateTables(){
		foreach(Component component in this.actions){
			string name = component.GetType().ToString();
			bool tableExists = this.data.Any(x=>x.name==name);
			if(!tableExists){
				StateTable table = new StateTable(name,component);
				this.data.Add(table);
			}
		}
		this.data = this.data.OrderBy(x=>x.name).ToList();
	}
	public void UpdateRequirements(){
		foreach(StateTable table in this.data){
			foreach(Component component in this.actions){
				string name = component.GetType().ToString();
				bool requirementExists = table.requirements.Any(x=>x.name==name);
				if(!requirementExists && name != table.name){
					StateRequirement requirement = new StateRequirement(name,component);
					table.requirements.Add(requirement);
				}
			}
			foreach(StateRequirement requirement in table.requirements.Copy()){
				if(requirement.name == table.name){
					table.requirements.Remove(requirement);
				}
			}
			table.requirements = table.requirements.OrderBy(x=>x.name).ToList();
		}
	}
}
