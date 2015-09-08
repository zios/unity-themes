using Zios;
using UnityEngine;
using System;
namespace Zios{
    [Serializable][AddComponentMenu("")]
    public class StateMonoBehaviour : ManagedMonoBehaviour{
	    [Internal] public string id;
	    [Internal] public AttributeBool ready = false;
	    [Internal] public AttributeBool usable = false;
	    [Internal] public AttributeBool inUse = false;
	    [Internal] public AttributeBool used = false;
	    public override void Awake(){
		    base.Awake();
		    this.ready.Setup("Ready",this);
		    this.usable.Setup("Usable",this);
		    this.inUse.Setup("Active",this);
		    this.used.Setup("Used",this);
	    }
	    [ContextMenu("Toggle Breakdown")]
	    public virtual void ToggleLinkBreakdown(){
			Utility.ToggleEditorPref("StateLinkBreakdownVisible",true);
		}
	    public void DefaultAlias(string name){
		    if(this.alias.IsEmpty()){
			    this.alias = name;
		    }
	    }
	    public virtual void Use(){}
	    public virtual void End(){}
	    public virtual void Toggle(bool state){}
    }
}
