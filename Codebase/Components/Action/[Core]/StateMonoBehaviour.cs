using Zios;
using UnityEngine;
using System;
[Serializable]
public class StateMonoBehaviour : ManagedMonoBehaviour{
	[HideInInspector] public string id;
	[HideInInspector] public AttributeBool requirable = true;
	[NonSerialized] public AttributeBool ready = true;
	[NonSerialized] public AttributeBool usable = false;
	[NonSerialized] public AttributeBool inUse = false;
	[NonSerialized] public AttributeBool used = false;
	public override void Awake(){
		base.Awake();
		this.inUse.Setup("Active",this);
		this.requirable.Setup("Requirable",this);
		this.ready.Setup("Ready",this);
		this.usable.Setup("Usable",this);
		this.used.Setup("Usable",this);
	}
	public virtual void Use(){}
	public virtual void End(){}
	public virtual void Toggle(bool state){}
}