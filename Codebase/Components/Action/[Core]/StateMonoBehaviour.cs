using Zios;
using UnityEngine;
using System;
[Serializable][AddComponentMenu("")]
public class StateMonoBehaviour : ManagedMonoBehaviour{
	[HideInInspector] public string id;
	[HideInInspector] public AttributeBool requirable = true;
	[HideInInspector] public AttributeBool ready = true;
	[HideInInspector] public AttributeBool usable = false;
	[HideInInspector] public AttributeBool inUse = false;
	[HideInInspector] public AttributeBool used = false;
	private bool requirableOverride;
	public override void Awake(){
		base.Awake();
		this.inUse.Setup("Active",this);
		this.requirable.Setup("Requirable",this);
		this.ready.Setup("Ready",this);
		this.usable.Setup("Usable",this);
		this.used.Setup("Used",this);
	}
	[ContextMenu("Toggle Column Visibility")]
	public void ToggleRequire(){
		this.requirable.Set(!this.requirable);
		this.requirableOverride = !this.requirableOverride;
		this.gameObject.Call("@Refresh");
	}
	public void DefaultAlias(string name){
		if(this.alias.IsEmpty()){
			this.alias = name;
		}
	}
	public void DefaultRequirable(bool state){
		if(!this.requirableOverride){
			this.requirable.Set(state);
		}
	}
	public virtual void Use(){}
	public virtual void End(){}
	public virtual void Toggle(bool state){}
}