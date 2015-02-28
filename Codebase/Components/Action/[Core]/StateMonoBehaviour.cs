using Zios;
using UnityEngine;
using System;
namespace Zios{
    [Serializable][AddComponentMenu("")]
    public class StateMonoBehaviour : ManagedMonoBehaviour{
	    [HideInInspector] public string id;
	    [HideInInspector] public AttributeBool requirable = true;
	    [HideInInspector] public AttributeBool ready = false;
	    [HideInInspector] public AttributeBool usable = false;
	    [HideInInspector] public AttributeBool inUse = false;
	    [HideInInspector] public AttributeBool used = false;
	    [HideInInspector] public AttributeBool endWhileUnusable = false;
	    private bool requirableOverride;
	    public override void Awake(){
		    base.Awake();
		    this.requirable.Setup("Requirable",this);
		    this.ready.Setup("Ready",this);
		    this.usable.Setup("Usable",this);
		    this.inUse.Setup("Active",this);
		    this.used.Setup("Used",this);
		    this.endWhileUnusable.Setup("End While Unusable",this);
	    }
	    [ContextMenu("Toggle Column In Table")]
	    public void ToggleRequire(){
		    this.requirable.Set(!this.requirable);
		    this.requirableOverride = !this.requirableOverride;
		    this.gameObject.Call("@Refresh");
	    }
	    [ContextMenu("//")]
	    public void Nothing(){}
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
}