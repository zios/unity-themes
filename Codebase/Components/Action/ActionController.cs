using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Action = Zios.Action;
using ActionPart = Zios.ActionPart;
[RequireComponent(typeof(Action))]
[ExecuteInEditMode][AddComponentMenu("Zios/Component/Action/Action Controller")]
public class ActionController : StateController{
	private Action action;
	public override void Reset(){
		this.action = null;
		base.Reset();
	}
	public override void Awake(){
		bool setup = false;
		if(this.action == null){
			this.action = this.GetComponent<Action>();
			setup = true;
		}
		if(setup){
			Events.Add("UpdateParts",this.UpdateStates);
			if(Application.isEditor){
				this.UpdateBase();
				this.UpdateScripts("ActionPart");
				this.UpdateRows("@Use","@End");
				this.UpdateRequirements("@Usable","@InUse");
				this.UpdateBaseRequirements();
			}
			this.UpdateOrder();
		}
	}
	public void UpdateOrder(){
		Dictionary<StateRow,int> data = new Dictionary<StateRow,int>();	
		List<StateRow> result = new List<StateRow>();
		foreach(StateRow row in this.table){
			if(row.name[0] == '@'){
				if(row.name == "@Use"){data[row] = 10;}
				if(row.name == "@End"){data[row] = 20;}
				continue;
			}
			data[row] = ((ActionPart)row.target).priority;
		}
		foreach(var item in data.OrderBy(x=>x.Value)){
			result.Add(item.Key);
		}
		this.table = result.ToArray();
	}
	public override void UpdateStates(){
		Dictionary<int,bool> isEndable = new Dictionary<int,bool>();
		Dictionary<int,bool> isUsable = new Dictionary<int,bool>();
		Dictionary<int,bool> isReady = new Dictionary<int,bool>();
		foreach(StateRow row in this.table){
			isReady.Clear();
			isUsable.Clear();
			bool isAction = row.name[0] == '@';
			bool useRow = row.name == "@Use";
			bool endRow = row.name == "@End";
			StateInterface script = isAction ? this.action : row.target;
			foreach(StateRequirement requirement in row.requirements){
				StateInterface target = requirement.name[0] == '@' ? this.action : requirement.target;
				int index = requirement.index;
				bool noRequirements = !requirement.requireOn && !requirement.requireOff;
				if(noRequirements){index = requirement.index = 0;}
				if(!isEndable.ContainsKey(index)){isEndable[index] = true;}
				if(!isUsable.ContainsKey(index)){isUsable[index] = true;}
				if(!isReady.ContainsKey(index)){isReady[index] = true;}
				if(noRequirements){continue;}
				if(!isEndable[index] && endRow){continue;}
				if(!isUsable[index] && !endRow){continue;}
				if(!isReady[index] && useRow){continue;}
				bool state = requirement.name == "@Usable" ? target.usable : target.inUse;
				bool mismatchOn = requirement.requireOn && !state;
				bool mismatchOff = requirement.requireOff && state;
				bool usable = !(mismatchOn || mismatchOff);
				if(useRow){isReady[index] = usable;}
				else if(!endRow){isUsable[index] = usable;}
				else{isEndable[index] = usable;}
			}
			if(useRow){script.ready = isReady.ContainsValue(true);}
			else if(!endRow){script.usable = isUsable.ContainsValue(true);}
			if(!script.usable && script.inUse && !isAction){
				script.End();
			}
			if(isEndable.ContainsValue(true) && endRow){
				this.action.End();
			}
		}
	}
	public void UpdateBase(){
		List<StateRow> rows = new List<StateRow>(this.table);
		StateInterface script = (StateInterface)this.action;
		if(!this.table.Any(x=>x.name=="@End")){rows.Insert(0,new StateRow("@End",script,this));}
		if(!this.table.Any(x=>x.name=="@Use")){rows.Insert(0,new StateRow("@Use",script,this));}
		this.table = rows.ToArray();
	}
	public void UpdateBaseRequirements(){
		StateInterface script = (StateInterface)this.action;
		foreach(StateRow row in this.table.Copy()){
			List<StateRequirement> requirements = new List<StateRequirement>(row.requirements);
			if(!row.requirements.Any(x=>x.name=="@Usable")){requirements.Insert(0,new StateRequirement("@Usable",script,this));}
			if(!row.requirements.Any(x=>x.name=="@InUse")){requirements.Insert(0,new StateRequirement("@InUse",script,this));}
			int index = requirements.FindIndex(x=>x.name=="@Usable");
			requirements.Move(index,0);
			row.requirements = requirements.ToArray();
		}
	}
}