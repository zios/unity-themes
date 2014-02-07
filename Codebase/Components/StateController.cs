using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode][AddComponentMenu("Zios/Component/General/State Controller")]
public class StateController : MonoBehaviour{
	public StateTable[] data = new StateTable[0];
	public List<MonoBehaviour> actions = new List<MonoBehaviour>();
	public void Awake(){
		Events.Add("UpdateStates",this.UpdateStates);
		if(Application.isEditor){
			this.UpdateActions();
			this.UpdateTables();
			this.UpdateRequirements();
		}
	}
	public void UpdateStates(){
		foreach(StateTable table in this.data){
			foreach(StateRequirement requirement in table.requirements){
				StateInterface action = table.self;
				StateInterface target = requirement.target;
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
		MonoBehaviour[] all = this.gameObject.GetComponents<MonoBehaviour>();
		foreach(MonoBehaviour script in all){
			if(script is StateInterface){
				this.actions.Add(script);
			}
		}
		this.actions = this.actions.OrderBy(x=>x.name).ToList();
	}
	public void UpdateTables(){
		List<StateTable> tables = new List<StateTable>(this.data);
		foreach(StateTable table in tables.Copy()){
			MonoBehaviour action = this.actions.Find(x=>x.GetGUID()==table.id);
			if(action == null){
				tables.Remove(table);
				string tableInfo = table.name + " [" + table.id + "]";
				Debug.Log("StateController : Removing old table -- " + tableInfo);
				continue;
			}
			table.name = action.GetType().ToString();
		}
		foreach(MonoBehaviour action in this.actions){
			string name = action.GetType().ToString();
			string guid = action.GetGUID();
			StateTable table = tables.Find(x=>x.id==guid);
			if(table == null){
				table = new StateTable(name,guid,action);
				tables.Add(table);
				Debug.Log("StateController : Creating table -- " + table.name);
			}
			else{
				table.name = name;
				table.self = (StateInterface)action;
				//Debug.Log("StateController : Updating table -- " + table.name);
			}
		}
		this.data = tables.OrderBy(x=>x.name).ToArray();
	}
	public void UpdateRequirements(){
		foreach(StateTable table in this.data){
			List<StateRequirement> requirements = new List<StateRequirement>(table.requirements);
			foreach(StateRequirement requirement in requirements.Copy()){
				MonoBehaviour action = this.actions.Find(x=>x.GetGUID()==requirement.id);
				if(action == null){
					requirements.Remove(requirement);
					Debug.Log("StateController : Removing old requirement -- " + requirement.name);
				}
			}
			foreach(MonoBehaviour action in this.actions){
				string name = action.GetType().ToString();
				string guid = action.GetGUID();
				StateRequirement requirement = requirements.Find(x=>x.id==guid);
				if(requirement == null){
					requirement = new StateRequirement(name,guid,action);
					requirements.Add(requirement);
					Debug.Log("StateController : Creating requirement -- " + table.name + "-" + name);
				}
				else{
					requirement.name = name;
					requirement.target = (StateInterface)action;
					//Debug.Log("StateController : Updating requirement -- " + table.name + "-" + name);
				}
			}
			table.requirements = requirements.OrderBy(x=>x.name).ToArray();
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
public class StateMonoBehaviour : MonoBehaviour,StateInterface{
	public bool stateUsable;
	public bool stateInUse;
	public bool usable{get{return this.stateUsable;}set{this.stateUsable = value;}}
	public bool inUse{get{return this.stateInUse;}set{this.stateInUse = value;}}
	public virtual void Use(){}
	public virtual void End(){}
	public virtual void Toggle(bool state){}
}
[Serializable]
public class StateTable{
	public string name;
	public bool endIfUnusable = false;
	public StateRequirement[] requirements = new StateRequirement[0];
	[HideInInspector] public string id;
	[HideInInspector] public StateInterface self;
	public StateTable(string name,string guid,MonoBehaviour script){
		this.name = name;
		this.id = guid;
		this.self = (StateInterface)script;
	}
}
[Serializable]
public class StateRequirement{
	public string name;
	public bool requireOn;
	public bool requireOff;
	[HideInInspector] public string id;
	[HideInInspector] public StateInterface target;
	public StateRequirement(string name,string guid,MonoBehaviour script,bool requireOn=false,bool requireOff=false){
		this.name = name;
		this.id = guid;
		this.requireOn = requireOn;
		this.requireOff = requireOff;
		this.target = (StateInterface)script;
	}
}