using Zios;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
[ExecuteInEditMode][AddComponentMenu("Zios/Component/Action/State Controller")]
public class StateController : MonoBehaviour{
	public int total;
	public StateRow[] table = new StateRow[0];
	public StateRow[] tableOff = new StateRow[0];
	public bool advanced;
	public List<StateInterface> scripts = new List<StateInterface>();
	public List<StateRow[]> tables = new List<StateRow[]>();
	public virtual void Reset(){
		this.table = new StateRow[0];
		this.tableOff = new StateRow[0];
		this.Awake();
	}
	public virtual void OnValidate(){this.Awake();}
	public virtual void OnEnable(){this.Awake();}
	public virtual void Awake(){
		Events.Add("@UpdateStates",this.UpdateStates);
		Events.Add("@Refresh",this.Refresh);
		this.Refresh();
	}
	[ContextMenu("Refresh")]
	public virtual void Refresh(){
		this.UpdateTableList();
		this.UpdateScripts();
		this.ResolveDuplicates();
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
		this.UpdateTable(this.table);
		if(this.advanced){
			this.UpdateTable(this.tableOff,true);
		}
	}
	public void UpdateTable(StateRow[] table,bool negative=false){
		foreach(StateRow row in table){
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
			if(this.advanced && usable){
				script.usable = negative ? false : true;
			}
			else if(!this.advanced){
				script.usable = usable;
			}
		}
	}
	public void UpdateTableList(){
		this.tables.Clear();
		this.tables.Add(this.table);
		this.tables.Add(this.tableOff);
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
	}
	public virtual void ResolveDuplicates(){
		foreach(StateRow[] table in this.tables){
			foreach(StateRow row in table){
				List<StateInterface> entries = this.scripts.FindAll(x=>x.id==row.id);
				foreach(StateInterface entry in entries.Skip(1)){
					bool hasName = !entry.alias.IsEmpty() && !row.name.IsEmpty();
					Debug.Log("StateController : Resolving duplicate ID [" + row.name + "]",(UnityObject)row.target);
					if(hasName && this.scripts.FindAll(x=>x.alias==row.name).Count > 1){
						row.name = entry.alias = row.name + "2";
					}
					row.id = entry.id = Guid.NewGuid().ToString();
				}
			}
		}
	}
	public virtual void UpdateRows(){
		for(int index=0;index<this.tables.Count;++index){
			StateRow[] table = this.tables[index];
			List<StateRow> rows = new List<StateRow>(table);
			this.RemoveEmptyAlternatives();
			this.RemoveDuplicates<StateRow>(rows);
			this.RemoveUnmatched<StateRow>(rows);
			this.AddUpdate<StateRow>(rows);
			this.RemoveNull<StateRow>(rows);
			if(index == 0){this.table = rows.ToArray();}
			if(index == 1){this.tableOff = rows.ToArray();}
		}
		this.UpdateTableList();
	}
	public virtual void UpdateRequirements(){
		List<string> hidden = new List<string>();
		foreach(StateRow[] table in this.tables){
			foreach(StateRow row in table){
				if(!row.target.requirable){
					hidden.Add(row.target.alias);
				}
			}
			foreach(StateRow row in table){
				foreach(StateRowData rowData in row.requirements){
					List<StateRequirement> requirements = new List<StateRequirement>(rowData.data);
					this.RemoveDuplicates<StateRequirement>(requirements);
					this.RemoveUnmatched<StateRequirement>(requirements);
					this.AddUpdate<StateRequirement>(requirements,hidden.ToArray());
					this.RemoveNull<StateRequirement>(requirements);
					rowData.data = requirements.ToArray();
				}
			}
		}
		this.RemoveHidden();
	}
	public virtual void UpdateOrder(){
		for(int index=0;index<this.tables.Count;++index){
			StateRow[] table = this.tables[index];
			List<StateRow> data = new List<StateRow>(table);
			foreach(StateRow row in table){
				int rowIndex = table.IndexOf(row);
				foreach(StateRowData rowData in row.requirements){
					int dataIndex = row.requirements.IndexOf(rowData);
					data[rowIndex].requirements[dataIndex].data = rowData.data.OrderBy(x=>x.name).ToArray();
				}
			}
			if(index == 0){this.table = data.OrderBy(x=>x.target.alias).ToArray();}
			if(index == 1){this.tableOff = data.OrderBy(x=>x.target.alias).ToArray();}
		}
	}
	// =============================
	//  Internal
	// =============================
	private void RemoveEmptyAlternatives(){
		foreach(StateRow[] table in this.tables){
			foreach(StateRow row in table){
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
						Debug.Log("StateController : Removing empty alternate row in -- " + row.name,(UnityObject)row.target);
						cleaned.Remove(rowData);
					}
					lastDataExists = !empty;
				}
				row.requirements = cleaned.ToArray();
			}
		}
	}
	private void RemoveHidden(){
		List<string> hidden = new List<string>();
		foreach(StateRow[] table in this.tables){
			foreach(StateRow row in table){
				if(!row.target.requirable){
					hidden.Add(row.target.alias);
				}
			}
			foreach(StateRow row in table){
				foreach(StateRowData rowData in row.requirements.Copy()){
					int dataIndex = row.requirements.IndexOf(rowData);
					List<StateRequirement> cleaned = new List<StateRequirement>(rowData.data);
					foreach(StateRequirement requirement in rowData.data){
						if(hidden.Contains(requirement.name)){
							Debug.Log("StateController : Removing non-requirable column  -- " + requirement.name,(UnityObject)requirement.target);
							cleaned.Remove(requirement);
						}
					}
					row.requirements[dataIndex].data = cleaned.ToArray();
				}
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
					Debug.LogError("StateController : (Deprecate!) Removing duplicate " + typeName + " -- " + targetA.name);
				}
			}
		}
	}
	private void RemoveUnmatched<T>(List<T> items) where T : StateBase{
		string typeName = typeof(T).ToString();
		foreach(T item in items.Copy()){
			StateInterface match = this.scripts.Find(x=>x.id==item.id);
			if(match == null){
				items.Remove(item);
				string itemInfo = typeName + " -- " + item.name + " [" + item.id + "]";
				Debug.Log("StateController : Removing old " + itemInfo);
			}
		}
	}
	private void RemoveNull<T>(List<T> items) where T : StateBase{
		string typeName = typeof(T).ToString();
		foreach(T item in items.Copy()){
			if(item.target == null){
				items.Remove(item);
				string itemInfo = typeName + " -- " + item.name + " [" + item.id + "]";
				Debug.LogError("StateController : (Deprecate!) Removing null " + itemInfo);
			}
		}
	}
	private void AddUpdate<T>(List<T> items,string[] ignore=null) where T : StateBase,new(){
		string typeName = typeof(T).ToString();
		foreach(StateInterface script in this.scripts){
			string name = script.alias.IsEmpty() ? script.GetType().ToString() : script.alias;
			ignore = ignore ?? new string[0];
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
	AttributeBool requirable{get;set;}
	AttributeBool ready{get;set;}
	AttributeBool usable{get;set;}
	AttributeBool inUse{get;set;}
	void Use();
	void End();
}
[Serializable]
public class StateMonoBehaviour : MonoBehaviour,StateInterface{
	public string stateAlias;
	[HideInInspector] public AttributeBool stateRequirable = true;
	[HideInInspector] public AttributeBool stateReady = true;
	[HideInInspector] public AttributeBool stateUsable = true;
	[HideInInspector] public AttributeBool stateInUse = true;
	[HideInInspector] public string stateID = Guid.NewGuid().ToString();
	public string id{get{return this.stateID;}set{this.stateID = value;}}
	public string alias{get{return this.stateAlias;}set{this.stateAlias = value;}}
	public AttributeBool requirable{get{return this.stateRequirable;}set{this.stateRequirable.Set(value);}}
	public AttributeBool ready{get{return this.stateReady;}set{this.stateReady.Set(value);}}
	public AttributeBool usable{get{return this.stateUsable;}set{this.stateUsable.Set(value);}}
	public AttributeBool inUse{get{return this.stateInUse;}set{this.stateInUse.Set(value);}}
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