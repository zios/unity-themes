using UnityEngine;
using Zios;
using System;
using System.Linq;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/*/Action Table")]
    public class ActionTable : StateTable{
	    public ActionLink[] parts = new ActionLink[0];
	    public bool isFixed;
	    public override void Awake(){
		    this.parts = this.gameObject.GetComponents<ActionLink>();
		    this.isFixed = !this.parts.Exists(x=>x.rate == UpdateRate.Update);
			/*Events.Add("On Disable",()=>{
				this.parts.ForEach(x=>x.ApplyState(false));
				this.UpdateStates();
			},this);*/
			base.Awake();
	    }
	    [ContextMenu("Refresh")]
	    public override void Refresh(){
		    this.UpdateTableList();
		    this.UpdateScripts<ActionLink>();
		    this.ResolveDuplicates();
		    this.UpdateRows();
		    this.UpdateRequirements();
		    this.UpdateOrder();
			this.CallEvent("On State Refreshed");
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
    }
}