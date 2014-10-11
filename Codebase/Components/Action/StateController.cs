using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode][AddComponentMenu("Zios/Component/Action/State Controller")]
public class StateController : MonoBehaviour{
	public int total;
	public StateRow[] table = new StateRow[0];
	public List<StateInterface> scripts = new List<StateInterface>();
	public virtual void Reset(){
		this.table = new StateRow[0];
		this.Awake();
	}
	public virtual void OnValidate(){this.Awake();}
	public virtual void OnEnable(){this.Awake();}
	public virtual void Awake(){
		Events.Add("UpdateStates",this.UpdateStates);
		Events.Add("Refresh",this.Refresh);
		this.Refresh();
	}
	[ContextMenu("Refresh")]
	public virtual void Refresh(){
		this.UpdateScripts();
		this.UpdateRows();
		this.UpdateRequirements();
		this.UpdateOrder();
	}
	public virtual void Update(){
		if(Application.isEditor){
			int count = this.gameObject.GetComponentsInChildren<StateMonoBehaviour>().Length;
			if(count != this.total){
				this.total = count;
				this.Refresh();
			}
		}
	}
	// =============================
	//  Maintenence
	// =============================
	public virtual void UpdateStates(){
		foreach(StateRow row in this.table){
			bool usable = true;
			StateInterface script = row.target;
			foreach(StateRowData requirements in row.requirements){
				foreach(StateRequirement requirement in requirements.data){
					bool noRequirements = !requirement.requireOn && !requirement.requireOff;
					if(noRequirements){continue;}
					bool mismatchOn = requirement.requireOn && !requirement.target.inUse;
					bool mismatchOff = requirement.requireOff && requirement.target.inUse;
					usable = !(mismatchOn || mismatchOff);
					if(!usable){break;}
				}
				if(usable){break;}
			}
			script.usable = usable;
		}
	}
	public virtual void UpdateScripts(string stateType="State"){
		this.scripts.Clear();
		MonoBehaviour[] all = this.gameObject.GetComponentsInChildren<MonoBehaviour>(true);
		foreach(MonoBehaviour script in all){
			if(script is StateInterface){
				StateInterface common = (StateInterface)script;
				if(common.id.IsEmpty()){
					common.id = Guid.NewGuid().ToString();
				}
				if(common.GetInterfaceType() == stateType){
					this.scripts.Add(common);
				}
			}
		}
		this.scripts = this.scripts.Distinct().ToList();
		foreach(StateRow row in this.table){
			List<StateInterface> entries = this.scripts.FindAll(x=>x.id==row.id);
			foreach(StateInterface entry in entries.Skip(1)){
				bool hasName = !entry.alias.IsEmpty() && !row.name.IsEmpty();
				Debug.Log("StateController : Resolving duplicate ID [" + row.name + "]");
				if(hasName && this.scripts.FindAll(x=>x.alias==row.name).Count > 1){
					row.name = entry.alias = row.name + "2";
				}
				row.id = entry.id = Guid.NewGuid().ToString();
			}
		}
	}
	public virtual void UpdateRows(params string[] ignore){
		List<StateRow> rows = new List<StateRow>(this.table);
		this.RemoveEmptyAlternatives();
		this.RemoveDuplicates<StateRow>(rows);
		this.RemoveUnmatched<StateRow>(rows,ignore);
		this.AddUpdate<StateRow>(rows,new string[]{});
		this.RemoveNull<StateRow>(rows,ignore);
		this.table = rows.ToArray();
	}
	public virtual void UpdateRequirements(params string[] ignore){
		List<string> hidden = new List<string>();
		foreach(StateRow row in this.table){
			if(!row.target.requirable){
				hidden.Add(row.target.alias);
			}
		}
		foreach(StateRow row in this.table){
			foreach(StateRowData rowData in row.requirements){
				List<StateRequirement> requirements = new List<StateRequirement>(rowData.data);
				this.RemoveDuplicates<StateRequirement>(requirements);
				this.RemoveUnmatched<StateRequirement>(requirements,ignore);
				this.AddUpdate<StateRequirement>(requirements,hidden.ToArray());
				this.RemoveNull<StateRequirement>(requirements,ignore);
				rowData.data = requirements.ToArray();
			}
		}
		this.RemoveHidden();
	}
	public virtual void UpdateOrder(){
		List<StateRow> data = new List<StateRow>(this.table);
		foreach(StateRow row in this.table){
			int rowIndex = this.table.IndexOf(row);
			foreach(StateRowData rowData in row.requirements){
				int dataIndex = row.requirements.IndexOf(rowData);
				data[rowIndex].requirements[dataIndex].data = rowData.data.OrderBy(x=>x.name).ToArray();
			}
		}
		this.table = data.OrderBy(x=>x.target.alias).ToArray();
	}
	// =============================
	//  Internal
	// =============================
	private void RemoveEmptyAlternatives(){
		foreach(StateRow row in this.table){
			List<StateRowData> cleaned = new List<StateRowData>(row.requirements);
			bool lastDataExists = true;
			foreach(StateRowData rowData in row.requirements){
				bool empty = true;
				foreach(StateRequirement requirement in rowData.data){
					if(requirement.requireOn || requirement.requireOff){
						empty = false;
					}
				}
				if(empty && !lastDataExists){
					Debug.Log("StateController : Removing empty alternate row in -- " + row.name);
					cleaned.Remove(rowData);
				}
				lastDataExists = !empty;
			}
			row.requirements = cleaned.ToArray();
		}
	}
	private void RemoveHidden(){
		List<string> hidden = new List<string>();
		foreach(StateRow row in this.table){
			if(!row.target.requirable){
				hidden.Add(row.target.alias);
			}
		}
		foreach(StateRow row in this.table){
			foreach(StateRowData rowData in row.requirements.Copy()){
				int dataIndex = row.requirements.IndexOf(rowData);
				List<StateRequirement> cleaned = new List<StateRequirement>(rowData.data);
				foreach(StateRequirement requirement in rowData.data){
					if(hidden.Contains(requirement.name)){
						cleaned.Remove(requirement);
					}
				}
				row.requirements[dataIndex].data = cleaned.ToArray();
			}
		}
	}
	private void RemoveDuplicates<T>(List<T> items) where T : StateBase{
		string typeName = typeof(T).ToString();
		foreach(T targetA in items.Copy()){
			List<T> otherItems = items.Copy();
			otherItems.Remove(targetA);
			foreach(T targetB in otherItems){
				bool duplicateGUID = !targetA.id.IsEmpty() && targetA.id == targetB.id;
				bool duplicateName = !targetA.name.IsEmpty() && targetA.name == targetB.name;
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
	private void AddUpdate<T>(List<T> items,string[] ignore) where T : StateBase,new(){
		string typeName = typeof(T).ToString();
		foreach(StateInterface script in this.scripts){
			string name = script.alias.IsEmpty() ? script.GetType().ToString() : script.alias;
			if(ignore.Contains(name)){continue;}
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
	bool requirable{get;set;}
	bool ready{get;set;}
	bool usable{get;set;}
	bool inUse{get;set;}
	void Use();
	void End();
}
[Serializable]
public class StateMonoBehaviour : MonoBehaviour,StateInterface{
	public string stateAlias;
	[HideInInspector] public bool stateRequirable = true;
	[HideInInspector] public bool stateReady;
	[HideInInspector] public bool stateUsable;
	[HideInInspector] public bool stateInUse;
	[HideInInspector] public string stateID = Guid.NewGuid().ToString();
	public string id{get{return this.stateID;}set{this.stateID = value;}}
	public string alias{get{return this.stateAlias;}set{this.stateAlias = value;}}
	public bool requirable{get{return this.stateRequirable;}set{this.stateRequirable = value;}}
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
	public virtual void Setup(string name,StateInterface script,StateController controller){
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
	public StateRowData[] requirements = new StateRowData[1];
	public StateRow(){}
	public StateRow(string name="",StateInterface script=null,StateController controller=null){
		this.Setup(name,script,controller);
	}
	public override void Setup(string name="",StateInterface script=null,StateController controller=null){
		this.requirements[0] = new StateRowData();
		base.Setup(name,script,controller);
	}
}
[Serializable]
public class StateRowData{
	public StateRequirement[] data = new StateRequirement[0];
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