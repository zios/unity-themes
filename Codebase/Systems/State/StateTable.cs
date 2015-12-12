using Zios;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/State Table")]
	public class StateTable : StateMonoBehaviour{
		public static bool debug;
		public StateRow[] table = new StateRow[0];
		public StateRow[] tableOff = new StateRow[0];
		public bool manual;
		public bool advanced;
		public AttributeBool external = true;
		public List<StateMonoBehaviour> scripts = new List<StateMonoBehaviour>();
		public List<StateRow[]> tables = new List<StateRow[]>();
		[NonSerialized] public bool dirty;
		public override void Awake(){
			base.Awake();
			this.alias = this.gameObject.name.Contains("Main") ? this.transform.parent.name : this.gameObject.name;
			Events.Register("On State Updated",this);
			Events.Register("On State Refreshed",this);
			Events.Add("On State Update",this.UpdateStates,this);
			Events.Add("On State Refresh",this.Refresh,this);
			Events.Add("On Start",StateTable.RefreshTables);
			if(!Application.isPlaying){
				Events.Add("On Hierarchy Changed",StateTable.RefreshTables);
				Events.Add("On Components Changed",StateTable.RefreshTables,this.gameObject);
			}
			this.external.Setup("External",this);
			this.external.Set(true);
		}
		public override void Step(){
			base.Step();
			foreach(var script in this.scripts){
				if(!script.IsEnabled()){continue;}
				if(script.nextState != null && (bool)script.nextState != script.used){
					this.dirty = true;
					script.controller.dirty = true;
					if(script is StateTable){script.As<StateTable>().dirty = true;}
					script.Apply((bool)script.nextState);
				}
			}
			if(this.dirty){
				this.dirty = false;
				this.CallEvent("On State Update");
			}
		}
		public static void RefreshTables(){
			var tables = Locate.GetSceneComponents<StateTable>().Where(x=>!x.IsNull()).OrderBy(x=>x.GetPath().Length);
			tables.Reverse();
			foreach(var table in tables){
				table.Refresh();
				table.UpdateStates();
			}
		}
		[ContextMenu("Refresh (All)")]
		public void RefreshAll(){StateTable.RefreshTables();}
		[ContextMenu("Refresh")]
		public virtual void Refresh(){
			if(Application.isPlaying){return;}
			if(!this.controller.IsEnabled()){this.controller = null;}
			if(!this.IsEnabled()){return;}
			this.UpdateScripts();
			if(this.scripts.Count > 0){
				this.UpdateTableList();
				this.ResolveDuplicates();
				this.UpdateRows();
				this.UpdateRequirements();
				this.UpdateOrder();
			}
			this.CallEvent("On State Refreshed");
		}
		//=============================
		//  Maintenence
		//=============================
		public virtual void UpdateStates(){
			if(!Application.isPlaying){return;}
			if(this.advanced){this.UpdateTable(this.tableOff,true);}
			this.UpdateTable(this.table);
			Utility.SetDirty(this,false,true);
			this.CallEvent("On State Updated");
		}
		public void UpdateTable(StateRow[] table,bool endTable=false){
			bool isOwnerUsable = true;
			if(this.controller.IsEnabled() && !this.manual){
				bool startMismatch = !endTable && !this.external;
				bool endMismatch = endTable && !this.external;
				if(startMismatch || endMismatch){
					isOwnerUsable = false;
				}
			}
			foreach(StateRow row in table){
				bool isUsable = false;
				bool isEmpty = true;
				bool isTable = row.target is StateTable && row.target != this;
				StateMonoBehaviour script = row.target;
				if(!script.IsEnabled()){continue;}
				if(isOwnerUsable){
					foreach(StateRowData requirements in row.requirements){
						foreach(StateRequirement requirement in requirements.data){
							if(!requirement.target.IsEnabled()){continue;}
							bool isExternal = requirement.name == "@External";
							if(isExternal && !this.manual){continue;}
							bool noRequirements = !requirement.requireOn && !requirement.requireOff;
							if(noRequirements){continue;}
							bool state = requirement.target.active;
							if(requirement.target.nextState != null){state = (bool)requirement.target.nextState;}
							if(isExternal){state = this.external;}
							bool mismatchOn = requirement.requireOn && !state;
							bool mismatchOff = requirement.requireOff && state;
							isUsable = !(mismatchOn || mismatchOff);
							isEmpty = false;
							if(!isUsable){break;}
						}
						if(isUsable){break;}
					}
				}
				else{
					isUsable = endTable;
					isEmpty = false;
				}
				var usable = isTable ? row.target.As<StateTable>().external : script.usable;
				bool wasUsable = usable.Get();
				if(!endTable){isUsable = isUsable || isEmpty;}
				if(this.advanced && isUsable){
					usable.Set(endTable ? false : true);
				}
				else if(!this.advanced){
					usable.Set(isUsable);
				}
				bool changes = isUsable != wasUsable;
				if(changes && isTable){
					var tableTarget = row.target.As<StateTable>();
					if(tableTarget.IsEnabled()){
						tableTarget.dirty = true;
					}
				}
			}
		}
		public void UpdateTableList(){
			this.tables.Clear();
			this.tables.Add(this.table);
			this.tables.Add(this.tableOff);
		}
		public virtual void UpdateScripts(){
			this.scripts.Clear();
			this.ScanTarget(this.gameObject);
			this.scripts = this.scripts.Distinct().ToList();
			this.rate = !this.scripts.Exists(x=>x.rate == UpdateRate.Update) ? UpdateRate.FixedUpdate : UpdateRate.Update;
		}
		public void ScanTarget(GameObject target){
			var states = Locate.GetObjectComponents<StateTable>(target).Cast<StateMonoBehaviour>().ToArray();
			bool keepSearching = states.Length < 1 || states[0] == this;
			if(states.Contains(this) && states.Length == 1 || states.Length == 0){
				states = Locate.GetObjectComponents<StateMonoBehaviour>(target);
			}
			foreach(var state in states){
				if(state.id.IsEmpty()){
					state.id = state.GetInstanceID().ToString();
				}
				if(state != this){state.controller = this;}
				this.scripts.Add(state);
			}
			if(keepSearching){
				foreach(var transform in target.transform){
					var current = transform.As<Component>().gameObject;
					if(current != target){
						this.ScanTarget(current);
					}
				}
			}
		}
		public virtual void ResolveDuplicates(){
			foreach(StateRow[] table in this.tables){
				foreach(StateRow row in table){
					List<StateMonoBehaviour> entries = this.scripts.FindAll(x=>x.id==row.id);
					foreach(StateMonoBehaviour entry in entries.Skip(1)){
						bool hasName = !entry.alias.IsEmpty() && !row.name.IsEmpty();
						if(StateTable.debug){Debug.Log("[StateTable] Resolving duplicate ID [" + row.name + "]",(UnityObject)row.target);}
						if(hasName && this.scripts.FindAll(x=>x.alias==row.name).Count > 1){
							row.name = entry.alias = row.name + "2";
						}
						row.id = entry.id = entry.GetInstanceID().ToString();
					}
				}
			}
		}
		public virtual void UpdateRows(){
			for(int index=0;index<this.tables.Count;++index){
				List<StateRow> rows = new List<StateRow>(this.tables[index]);
				this.RemoveDuplicates<StateRow>(ref rows);
				this.RepairUnmatched<StateRow>(ref rows);
				this.AddUpdate<StateRow>(ref rows);
				this.RemoveNull<StateRow>(ref rows);
				if(index == 0){this.table = rows.ToArray();}
				if(index == 1){this.tableOff = rows.ToArray();}
			}
			this.UpdateTableList();
		}
		public virtual void UpdateRequirements(){
			foreach(StateRow[] table in this.tables){
				foreach(StateRow row in table){
					if(this.controller.IsEnabled()){
						foreach(StateRowData rowData in row.requirements){
							var external = rowData.data.Where(x=>x.name=="@External").FirstOrDefault();
							if(external.IsNull()){
								external = new StateRequirement("@External",this.controller,this);
								external.requireOn = table == this.tables[0];
								external.requireOff = table == this.tables[1];
								rowData.data = rowData.data.Add(external);
							}
							external.target = this.controller;
						}
					}
					foreach(StateRowData rowData in row.requirements){
						List<StateRequirement> requirements = new List<StateRequirement>(rowData.data);
						this.RemoveDuplicates<StateRequirement>(ref requirements);
						this.RepairUnmatched<StateRequirement>(ref requirements);
						this.AddUpdate<StateRequirement>(ref requirements);
						this.RemoveNull<StateRequirement>(ref requirements);
						rowData.data = requirements.ToArray();
					}
				}
			}
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
				if(index == 0){this.table = data.OrderBy(x=>x.name).ToArray();}
				if(index == 1){this.tableOff = data.OrderBy(x=>x.name).ToArray();}
			}
		}
		//=============================
		//  Internal
		//=============================
		private void RemoveDuplicates<T>(ref List<T> items) where T : StateBase{
			string typeName = typeof(T).ToString();
			foreach(T targetA in items.Copy()){
				foreach(T targetB in items.Copy()){
					if(targetA == targetB){continue;}
					bool duplicateGUID = !targetA.id.IsEmpty() && targetA.id == targetB.id;
					bool duplicateName = !targetA.name.IsEmpty() && targetA.name == targetB.name;
					bool duplicateSource = targetA.name == "@Active" || targetB.name == "@Active";
					if(duplicateGUID && (duplicateSource || duplicateName)){
						var removeTarget = duplicateSource && targetA.name == "@Active" ? targetB : targetA;
						if(items.Contains(removeTarget)){
							items.Remove(removeTarget);
							Debug.LogWarning("[StateTable] Removing duplicate " + typeName + " -- " + removeTarget.name,this.gameObject);
						}
					}
				}
			}
		}
		private void RepairUnmatched<T>(ref List<T> items) where T : StateBase{
			string typeName = typeof(T).ToString();
			foreach(T item in items.Copy()){
				if(this.controller.IsEnabled() && item.name == "@External"){continue;}
				StateMonoBehaviour match = this.scripts.Find(x=>x.id==item.id);
				if(match == null){
					match = this.scripts.Find(x=>x.alias==item.name);
					if(match != null){
						item.id = match.id;
						item.target = match;
					}
					else{items.Remove(item);}
					if(StateTable.debug){
						string itemInfo = typeName + " -- " + item.name + " [" + item.id + "]";
						string action = match == null ? "Removing" : "Repairing";
						Debug.Log("[StateTable] " + action + " old " + itemInfo,this.gameObject);
					}
				}
			}
		}
		private void RemoveNull<T>(ref List<T> items) where T : StateBase{
			string typeName = typeof(T).ToString();
			foreach(T item in items.Copy()){
				if(item.target == null){
					items.Remove(item);
					string itemInfo = typeName + " -- " + item.name + " [" + item.id + "]";
					if(StateTable.debug){Debug.Log("[StateTable] Removing null " + itemInfo,this.gameObject);}
				}
			}
		}
		private void AddUpdate<T>(ref List<T> items,string[] ignore=null) where T : StateBase,new(){
			ignore = ignore ?? new string[0];
			string typeName = typeof(T).ToString();
			foreach(StateMonoBehaviour script in this.scripts){
				string name = script.alias.IsEmpty() ? script.GetType().ToString() : script.alias;
				if(ignore.Contains(name)){continue;}
				T item = items.Find(x=>x.id==script.id);
				if(item != null && this.scripts.FindAll(x=>x.id==item.id).Count > 1){
					item = items.Find(x=>x.name==name);
				}
				if(script == this){name = "@Active";}
				if(item == null){
					item = new T();
					item.Setup(name,script,this);
					items.Add(item);
					if(StateTable.debug){
						string itemInfo = typeName + " -- " + item.name + " [" + item.id + "]";
						Debug.Log("[StateTable] Creating " + itemInfo,this);
					}
				}
				else{
					if(item.target == this){name = "@Active";}
					item.name = name;
					item.target = script;
					//if(StateTable.debug){Debug.Log("[StateTable] Updating " + typeName + " -- " + item.name);}
				}
			}
		}
	}
	[Serializable]
	public class StateBase{
		public string name;
		public StateTable stateTable;
		[Internal] public string id;
		[Internal] public StateMonoBehaviour target;
		public virtual void Setup(string name,StateMonoBehaviour script,StateTable stateTable){
			this.name = name;
			this.stateTable = stateTable;
			if(script != null){
				this.id = script.id;
				this.target = script;
			}
		}
	}
	[Serializable]
	public class StateRow : StateBase{
		public bool empty;
		public string section;
		public StateRowData[] requirements = new StateRowData[1];
		//public StateRequirement[] fields = new StateRequirement[0];
		public StateRow(){}
		public StateRow(string name="",StateMonoBehaviour script=null,StateTable stateTable=null){
			this.Setup(name,script,stateTable);
		}
		public override void Setup(string name="",StateMonoBehaviour script=null,StateTable stateTable=null){
			this.requirements[0] = new StateRowData();
			base.Setup(name,script,stateTable);
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
		public StateRequirement(string name="",StateMonoBehaviour script=null,StateTable stateTable=null){
			this.Setup(name,script,stateTable);
		}
	}
}