using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode][AddComponentMenu("Zios/Component/General/State Controller")]
public class StateController : MonoBehaviour{
	public TruthTable<StateInterface,StateRow<StateInterface,StateRequirement<StateInterface>>,StateRequirement<StateInterface>> table;
	public virtual void Awake(){
		if(this.table == null){
			this.table = new TruthTable<StateInterface,StateRow<StateInterface,StateRequirement<StateInterface>>,StateRequirement<StateInterface>>(this.gameObject);
		}
		else{
			Events.Add("UpdateStates",this.table.UpdateStates);
			if(Application.isEditor){
				this.table.UpdateActions();
				this.table.UpdateRows();
				this.table.UpdateRequirements();
			}
		}
	}
}
[Serializable]
public class TruthTable<Interface,Row,Requirement>
	where Interface : class,StateInterface
	where Row : StateRow<Interface,Requirement>,new()
	where Requirement : StateRequirement<Interface>,new(){
	public GameObject gameObject;
	public Row[] data = new Row[0];
	public List<Row> actions = new List<Row>();
	public TruthTable(GameObject gameObject){
		this.gameObject = gameObject;
	}
	public void UpdateStates(){
		foreach(Row row in this.data){
			foreach(Requirement requirement in row.requirements){
				Interface action = row.self;
				Interface target = requirement.target;
				bool state = target.inUse;
				bool mismatchOn = requirement.requireOn && !state;
				bool mismatchOff = requirement.requireOff && state;
				action.usable = !(mismatchOn || mismatchOff);
				if(!action.usable){
					if(row.endIfUnusable && action.inUse){
						action.End();
					}
					break;
				}
			}
		}
	}
	public void UpdateActions(params MonoBehaviour[] ignore){
		this.actions.Clear();
		MonoBehaviour[] all = this.gameObject.GetComponents<MonoBehaviour>();
		foreach(MonoBehaviour script in all){
			if(ignore.Contains(script)){continue;}
			if(script is Interface){
				string name = script.GetType().ToString();
				string guid = script.GetGUID();
				Row row = new Row();
				row.Setup(name,guid,(Interface)((object)script));
				this.actions.Add(row);
			}
		}
		this.actions = this.actions.OrderBy(x=>x.name).ToList();
	}
	public void UpdateRows(params string[] ignore){
		List<Row> rows = new List<Row>(this.data);
		foreach(Row row in rows.Copy()){
			if(ignore.Contains(row.name)){continue;}
			Row action = this.actions.Find(x=>x.id==row.id);
			if(action == null){
				rows.Remove(row);
				string rowInfo = row.name + " [" + row.id + "]";
				Debug.Log("StateController : Removing old row -- " + rowInfo);
				continue;
			}
			Interface common = (Interface)action.self;
			row.name = common.alias == "" ? action.GetType().ToString() : common.alias;
		}
		foreach(Row action in this.actions){
			Interface common = (Interface)action.self;
			string name = common.alias == "" ? action.GetType().ToString() : common.alias;
			string guid = action.id;
			Row row = rows.Find(x=>x.id==guid);
			if(row == null){
				row = new Row();
				row.Setup(name,guid,action.self);
				rows.Add(row);
				Debug.Log("StateController : Creating row -- " + row.name);
			}
			else{
				row.name = name;
				row.self = (Interface)action.self;
				//Debug.Log("StateController : Updating row -- " + row.name);
			}
		}
		this.data = rows.OrderBy(x=>x.name).ToArray();
	}
	public void UpdateRequirements(params string[] ignore){
		foreach(Row row in this.data){
			List<Requirement> requirements = new List<Requirement>(row.requirements);
			foreach(Requirement requirement in requirements.Copy()){
				if(ignore.Contains(requirement.name)){continue;}
				Row action = this.actions.Find(x=>x.id==requirement.id);
				if(action == null){
					requirements.Remove(requirement);
					Debug.Log("StateController : Removing old requirement -- " + requirement.name);
				}
			}
			foreach(Row action in this.actions){
				string name = action.GetType().ToString();
				string guid = action.id;
				Requirement requirement = requirements.Find(x=>x.id==guid);
				if(requirement == null){
					requirement = new Requirement();
					requirement.Setup(name,guid,action.self);
					requirements.Add(requirement);
					Debug.Log("StateController : Creating requirement -- " + row.name + "-" + name);
				}
				else{
					requirement.name = name;
					requirement.target = (Interface)action.self;
					//Debug.Log("StateController : Updating requirement -- " + row.name + "-" + name);
				}
			}
			row.requirements = requirements.OrderBy(x=>x.name).ToArray();
		}
	}
}
[Serializable]
public class StateRow<Interface,Requirement>
	where Interface : class,StateInterface
	where Requirement : StateRequirement<Interface>,new(){
	public string name;
	public bool endIfUnusable = false;
	public Requirement[] requirements = new Requirement[0];
	[HideInInspector] public string id;
	[HideInInspector] public Interface self;
	public StateRow(){}
	public StateRow(string name="",string guid="",Interface script=null){
		this.name = name;
		this.id = guid;
		this.self = (script == null) ? null : script;
	}
	public void Setup(string name,string guid,Interface script){
		this.name = name;
		this.id = guid;
		this.self = (script == null) ? null : script;
	}
}
[Serializable]
public class StateRequirement<Interface> where Interface : class,StateInterface{
	public string name;
	public bool requireOn;
	public bool requireOff;
	[HideInInspector] public string id;
	[HideInInspector] public Interface target;
	public StateRequirement(){}
	public StateRequirement(string name="",string guid="",Interface script=null,bool requireOn=false,bool requireOff=false){
		this.name = name;
		this.id = guid;
		this.requireOn = requireOn;
		this.requireOff = requireOff;
		this.target = (script == null) ? null : script;
	}
	public void Setup(string name,string guid,Interface script,bool requireOn=false,bool requireOff=false){
		this.name = name;
		this.id = guid;
		this.requireOn = requireOn;
		this.requireOff = requireOff;
		this.target = (script == null) ? null : script;
	}
}
[Serializable]
public class StateMonoBehaviour : MonoBehaviour,StateInterface{
	public string stateAlias;
	public bool stateUsable;
	public bool stateInUse;
	public string alias{get{return this.stateAlias;}set{this.stateAlias = value;}}
	public bool usable{get{return this.stateUsable;}set{this.stateUsable = value;}}
	public bool inUse{get{return this.stateInUse;}set{this.stateInUse = value;}}
	public virtual void Use(){}
	public virtual void End(){}
	public virtual void Toggle(bool state){}
}
public interface StateInterface{
	string alias{get;set;}
	bool usable{get;set;}
	bool inUse{get;set;}
	void Use();
	void End();
}