using Zios;
using UnityEngine;
using System;
namespace Zios{
    [Serializable][AddComponentMenu("")]
    public class StateMonoBehaviour : ManagedMonoBehaviour{
	    [Internal] public string id;
	    [Internal] public AttributeBool requirable = true;
	    [Internal] public AttributeBool ready = false;
	    [Internal] public AttributeBool usable = false;
	    [Internal] public AttributeBool inUse = false;
	    [Internal] public AttributeBool used = false;
	    private bool requirableOverride;
	    public override void Awake(){
		    base.Awake();
		    this.requirable.Setup("Requirable",this);
		    this.ready.Setup("Ready",this);
		    this.usable.Setup("Usable",this);
		    this.inUse.Setup("Active",this);
		    this.used.Setup("Used",this);
	    }
	    [ContextMenu("Toggle Column In Table")]
	    public void ToggleRequire(){
		    this.requirable.Set(!this.requirable);
		    this.requirableOverride = !this.requirableOverride;
		    this.gameObject.CallEvent("On State Refresh");
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
}
