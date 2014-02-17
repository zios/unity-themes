using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode][AddComponentMenu("Zios/Component/Action/State Controller")]
public class StateController : MonoBehaviour{
	public StateRow[] table = new StateRow[0];
	public List<StateInterface> scripts = new List<StateInterface>();
	public Dictionary<string,List<StateInterface>> duplicates = new Dictionary<string,List<StateInterface>>();
	public virtual void Reset(){
		this.table = new StateRow[0];
		this.Awake();
	}
	public virtual void OnValidate(){this.Awake();}
	public virtual void OnEnable(){this.Awake();}
	public virtual void Awake(){
		Events.Add("UpdateStates",this.UpdateStates);
		if(Application.isEditor){
			this.UpdateScripts();
			this.UpdateRows();
			this.UpdateRequirements();
		}
	}
	// =============================
	//  Maintenence
	// =============================
	public virtual void UpdateStates(){
		foreach(StateRow row in this.table){
			StateInterface script = row.target;
			foreach(StateRequirement requirement in row.requirements){
				StateInterface target = requirement.target;
				bool state = target.inUse;
				bool mismatchOn = requirement.requireOn && !state;
				bool mismatchOff = requirement.requireOff && state;
				script.usable = !(mismatchOn || mismatchOff);
				script.ready = script.usable;
				if(!script.usable){
					if(row.endIfUnusable && script.inUse){
						script.End();
					}
					break;
				}
			}
		}
	}
	public virtual void UpdateScripts(string stateType="State"){
		this.scripts.Clear();
		this.duplicates.Clear();
		MonoBehaviour[] all = this.gameObject.GetComponentsInChildren<MonoBehaviour>(true);
		foreach(MonoBehaviour script in all){
			if(script is StateInterface){
				StateInterface common = (StateInterface)script;
				if(common.id == ""){
					common.id = Guid.NewGuid().ToString();
				}
				if(common.GetInterfaceType() == stateType){
					this.scripts.Add(common);
				}
			}
		}
		foreach(StateInterface script in this.scripts){
			List<StateInterface> entries = this.scripts.FindAll(x=>x.id==script.id);
			if(entries.Count > 1){
				foreach(StateInterface entry in entries){
					if(!this.duplicates.ContainsKey(entry.id)){
						this.duplicates[entry.id] = new List<StateInterface>();
					}
					if(!this.duplicates[entry.id].Contains(entry)){
						this.duplicates[entry.id].Add(entry);
						Debug.LogWarning("Duplicate ID [" + entry.id + "] = " + entry.alias);
					}
				}
			}
		}
		this.scripts = this.scripts.Distinct().ToList();
	}
	public virtual void UpdateRows(params string[] ignore){
		List<StateRow> rows = new List<StateRow>(this.table);
		this.RemoveDuplicates<StateRow>(rows);
		this.RemoveUnmatched<StateRow>(rows,ignore);
		this.AddUpdate<StateRow>(rows);
		this.RemoveNull<StateRow>(rows,ignore);
		this.table = rows.ToArray();
	}
	public virtual void UpdateRequirements(params string[] ignore){
		foreach(StateRow row in this.table){
			List<StateRequirement> requirements = new List<StateRequirement>(row.requirements);
			this.RemoveDuplicates<StateRequirement>(requirements);
			this.RemoveUnmatched<StateRequirement>(requirements,ignore);
			this.AddUpdate<StateRequirement>(requirements);
			this.RemoveNull<StateRequirement>(requirements,ignore);
			row.requirements = requirements.ToArray();
		}
	}
	public virtual void RepairRow(StateRow row){
		row.id = row.target.id = Guid.NewGuid().ToString();
		foreach(StateRow entry in this.table){
			if(entry.target == row.target){entry.id = entry.target.id = row.target.id;}
			foreach(StateRequirement requirement in entry.requirements){
				if(requirement.target == row.target){requirement.id = requirement.target.id = row.target.id;}
			}
		}
	}
	// =============================
	//  Internal
	// =============================
	private void RemoveDuplicates<T>(List<T> items) where T : StateBase{
		string typeName = typeof(T).ToString();
		foreach(T targetA in items.Copy()){
			List<T> otherItems = items.Copy();
			otherItems.Remove(targetA);
			foreach(T targetB in otherItems){
				bool duplicateGUID = targetA.id != "" && targetA.id == targetB.id;
				bool duplicateName = targetA.name != "" && targetA.name == targetB.name;
				if(duplicateGUID && duplicateName){
					items.Remove(targetA);
					Debug.Log("StateController : Removing duplicate " + typeName + " -- " + targetA.name);
				}
			}
		}
	}
	private void RemoveUnmatched<T>(List<T> items,string[] ignore) where T : StateBase{
		string typeName = typeof(T).ToString();
		foreach(T item in items.Copy()){
			if(ignore.Contains(item.name)){continue;}
			StateInterface match = this.scripts.Find(x=>x.id==item.id);
			if(match == null){
				items.Remove(item);
				string itemInfo = typeName + " -- " + item.name + " [" + item.id + "]";
				Debug.Log("StateController : Removing old " + itemInfo);
			}
		}
	}
	private void RemoveNull<T>(List<T> items,string[] ignore) where T : StateBase{
		string typeName = typeof(T).ToString();
		foreach(T item in items.Copy()){
			if(ignore.Contains(item.name)){continue;}
			if(item.target == null){
				items.Remove(item);
				string itemInfo = typeName + " -- " + item.name + " [" + item.id + "]";
				Debug.Log("StateController : Removing null " + itemInfo);
			}
		}
	}
	private void AddUpdate<T>(List<T> items) where T : StateBase,new(){
		string typeName = typeof(T).ToString();
		foreach(StateInterface script in this.scripts){
			string name = script.alias == "" ? script.GetType().ToString() : script.alias;
			T item = items.Find(x=>x.id==script.id);
			if(item != null && this.scripts.FindAll(x=>x.id==item.id).Count > 1){
				item = items.Find(x=>x.name==name);
			}
			if(item == null){
				item = new T();
				item.Setup(name,script,this);
				items.Add(item);
				string itemInfo = typeName + " -- " + item.name + " [" + item.id + "]";
				Debug.Log("StateController : Creating " + itemInfo);
			}
			else{
				item.name = name;
				item.target = script;
				//Debug.Log("StateController : Updating " + typeName + " -- " + item.name);
			}
		}
	}
}
public interface StateInterface{
	string GetInterfaceType();
	string alias{get;set;}
	string id{get;set;}
	bool ready{get;set;}
	bool usable{get;set;}
	bool inUse{get;set;}
	void Use();
	void End();
}
[Serializable]
public class StateMonoBehaviour : MonoBehaviour,StateInterface{
	public string stateAlias;
	[HideInInspector] public bool stateReady;
	[HideInInspector] public bool stateUsable;
	[HideInInspector] public bool stateInUse;
	[HideInInspector] public string stateID = Guid.NewGuid().ToString();
	public string id{get{return this.stateID;}set{this.stateID = value;}}
	public string alias{get{return this.stateAlias;}set{this.stateAlias = value;}}
	public bool ready{get{return this.stateReady;}set{this.stateReady = value;}}
	public bool usable{get{return this.stateUsable;}set{this.stateUsable = value;}}
	public bool inUse{get{return this.stateInUse;}set{this.stateInUse = value;}}	
	public virtual string GetInterfaceType(){return "State";}
	public virtual void Use(){}
	public virtual void End(){}
	public virtual void Toggle(bool state){}
}
[Serializable]
public class StateBase{
	public string name;
	public StateController controller;
	[HideInInspector] public string id;
	[HideInInspector] public StateInterface target;
	public void Setup(string name,StateInterface script,StateController controller){
		this.name = name;
		this.controller = controller;
		if(script != null){
			this.id = script.id;
			this.target = script;
		}
	}
}
[Serializable]
public class StateRow : StateBase{
	public bool endIfUnusable = false;
	public StateRequirement[] requirements = new StateRequirement[0];
	public StateRow(){}
	public StateRow(string name="",StateInterface script=null,StateController controller=null){
		this.Setup(name,script,controller);
	}
}
[Serializable]
public class StateRequirement : StateBase{
	public bool requireOn;
	public bool requireOff;
	public StateRequirement(){}
	public StateRequirement(string name="",StateInterface script=null,StateController controller=null){
		this.Setup(name,script,controller);
	}
}