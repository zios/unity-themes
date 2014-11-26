#pragma warning disable 0414
using UnityEngine;
using Zios;
using System;
using Action = Zios.Action;
using ActionPart = Zios.ActionPart;
[AddComponentMenu("Zios/Component/Action/*/Action Controller")]
public class ActionController : StateController{
	public ActionPart[] parts = new ActionPart[0];
	public bool isFixed;
	public override void Awake(){
		this.parts = this.gameObject.GetComponents<ActionPart>();
		this.isFixed = !this.parts.Exists(x=>x.rate == UpdateRate.Update);
	}
	[ContextMenu("Refresh")]
	public override void Refresh(){
		this.UpdateTableList();
		this.UpdateScripts<ActionPart>(false);
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
		foreach(ActionPart part in this.parts){
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
			foreach(ActionPart part in this.parts){
				part.ApplyState(false);
			}
			this.UpdateStates();
		}
	}
}