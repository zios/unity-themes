using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
[Serializable]
public class StateTable{
	public string name;
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
[ExecuteInEditMode]
public class StateController : MonoBehaviour{
	public List<Component> actions = new List<Component>();
	public List<StateTable> data = new List<StateTable>();
	private Dictionary<string,bool> buffer = new Dictionary<string,bool>();
	public void Awake(){
		this.UpdateActions();
		this.UpdateTables();
		this.UpdateRequirements();
	}
	public void Update(){
		foreach(Component component in this.actions){
			CoreAction action = (CoreAction)component;
			string name = component.GetType().ToString();
			this.buffer[name] = action.inUse;
			action.usable = true;
		}
		foreach(StateTable table in this.data){
			foreach(StateRequirement requirement in table.requirements){
				bool state = this.buffer[requirement.name];
				bool mismatchOn = requirement.requireOn && !state;
				bool mismatchOff = requirement.requireOff && state;
				if(mismatchOn || mismatchOff){
					CoreAction action = (CoreAction)table.self;
					action.usable = false;
					if(action.inUse){
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
