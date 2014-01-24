using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor;
#endif
[ExecuteInEditMode][AddComponentMenu("Zios/Component/General/State Controller")]
public class StateController : MonoBehaviour{
	public List<StateTable> data = new List<StateTable>();
	public List<Component> actions = new List<Component>();
	public void Awake(){
		Events.Add("UpdateStates",this.UpdateStates);
		this.UpdateActions();
		this.UpdateTables();
		this.UpdateRequirements();
	}
	public void UpdateStates(){
		foreach(StateTable table in this.data){
			foreach(StateRequirement requirement in table.requirements){
				StateMonoBehaviour action = table.self;
				StateMonoBehaviour target = requirement.target;
				bool state = target.inUse;
				bool mismatchOn = requirement.requireOn && !state;
				bool mismatchOff = requirement.requireOff && state;
				action.usable = !(mismatchOn || mismatchOff);
				if(!action.usable){
					if(table.endIfUnusable && action.inUse){
						action.End();
					}
					break;
				}
			}
		}
	}
	public void UpdateActions(){
		this.actions.Clear();
		Component[] all = this.gameObject.GetComponents<Component>();
		foreach(Component component in all){
			bool applicable = component.HasAttribute("usable") && component.HasAttribute("inUse");
			if(applicable){
				this.actions.Add(component);
			}
		}
		this.actions = this.actions.OrderBy(x=>x.name).ToList();
	}
	public void UpdateTables(){
		foreach(StateTable table in this.data.Copy()){
			bool actionExists = this.actions.Any(x=>x.GetType().ToString()==table.name);
			if(!actionExists){
				this.data.Remove(table);
				Debug.Log("StateController : Removing old table entry -- " + table.name);
			}
		}
		foreach(Component component in this.actions){
			string name = component.GetType().ToString();
			bool tableExists = this.data.Any(x=>x.name==name);
			if(!tableExists){
				StateTable table = new StateTable(name,component);
				this.data.Add(table);
			}
			else{
				StateTable table = this.data.Find(x=>x.name==name);
				table.self = (StateMonoBehaviour)component;
				//Debug.Log("StateController : Updating table entry -- " + name);
			}
		}
		this.data = this.data.OrderBy(x=>x.name).ToList();
	}
	public void UpdateRequirements(){
		foreach(StateTable table in this.data){
			foreach(Component component in this.actions){
				string name = component.GetType().ToString();
				bool requirementExists = table.requirements.Any(x=>x.name==name);
				if(!requirementExists){
					StateRequirement requirement = new StateRequirement(name,component);
					table.requirements.Add(requirement);
				}
				else{
					StateRequirement requirement = table.requirements.Find(x=>x.name==name);
					requirement.target = (StateMonoBehaviour)component;
					//Debug.Log("StateController : Updating requirement entry -- " + name);
				}
			}
			foreach(StateRequirement requirement in table.requirements.Copy()){
				bool actionExists = this.actions.Any(x=>x.GetType().ToString()==requirement.name);
				if(!actionExists){
					table.requirements.Remove(requirement);
					Debug.Log("StateController : Removing old requirement entry -- " + requirement.name);
				}
			}
			table.requirements = table.requirements.OrderBy(x=>x.name).ToList();
		}
	}
}
public interface StateInterface{
	bool usable{get;set;}
	bool inUse{get;set;}
	void Use();
	void End();
}
[Serializable]
public class StateMonoBehaviour : MonoBehaviour{
	public bool usable;
	public bool inUse;
	public virtual void Use(){}
	public virtual void End(){}
}
[Serializable]
public class StateTable{
	public string name;
	public bool endIfUnusable = false;
	public List<StateRequirement> requirements = new List<StateRequirement>();
	[HideInInspector] public StateMonoBehaviour self;
	public StateTable(string name,Component component){
		this.name = name;
		this.self = (StateMonoBehaviour)component;
	}
}
[Serializable]
public class StateRequirement{
	public string name;
	public bool requireOn;
	public bool requireOff;
	//public string GUID;
	[HideInInspector] public StateMonoBehaviour target;
	public StateRequirement(string name,Component component,bool requireOn=false,bool requireOff=false){
		this.name = name;
		this.requireOn = requireOn;
		this.requireOff = requireOff;
		this.target = (StateMonoBehaviour)component;
	}
}