using Zios;
using UnityEngine;
using System;
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