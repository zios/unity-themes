#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/*/Action Table")]
    public class ActionTable : StateTable{
	    public ActionLink[] parts = new ActionLink[0];
	    public bool isFixed;
	    public override void Awake(){
		    this.parts = this.gameObject.GetComponents<ActionLink>();
		    this.isFixed = !this.parts.Exists(x=>x.rate == UpdateRate.Update);
	    }
	    [ContextMenu("Refresh")]
	    public override void Refresh(){
		    this.UpdateTableList();
		    this.UpdateScripts<ActionLink>(false);
		    this.ResolveDuplicates();
		    this.UpdateRows();
		    this.UpdateRequirements();
		    this.UpdateOrder();
	    }
	    public void Update(){
		    if(!this.isFixed){
			    this.Step();
		    }
	    }
	    public void FixedUpdate(){
		    if(this.isFixed){
			    this.Step();
		    }
	    }
	    public void Step(){
		    bool changes = false;
		    foreach(ActionLink part in this.parts){
			    if(part.used && part.inUse && part.occurrence == ActionOccurrence.Once){
				    part.inUse.Set(false);
				    changes = true;
			    }
			    if(part.nextState != null){
				    part.ApplyState((bool)part.nextState);
				    part.nextState = null;
				    changes = true;
			    }
		    }
		    if(changes){
			    this.UpdateStates();
		    }
	    }
	    public void OnDisable(){
		    if(!this.gameObject.activeInHierarchy || !this.enabled){
			    foreach(ActionLink part in this.parts){
				    part.ApplyState(false);
			    }
			    this.UpdateStates();
		    }
	    }
    }
}