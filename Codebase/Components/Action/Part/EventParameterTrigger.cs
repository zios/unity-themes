using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger (Parameter)")]
public class EventParameterTrigger : ActionPart{
	public string eventName;
	public string parameter;
	public ValueType parameterType;
	public EventFrequency eventFrequency;
	public GameObject eventTarget;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		base.Use();
		if(this.eventFrequency == EventFrequency.Once){
			if(!this.inUse){this.CallEvent();}
		}
		else{this.CallEvent();}
	}
	public void CallEvent(){
		string name = this.eventName;
		string value = this.parameter;
		ValueType type = this.parameterType;
		GameObject target = this.eventTarget == null ? this.action.owner : this.eventTarget;
		if(type == ValueType.String){target.Call(name,value);}
		if(type == ValueType.Int){target.Call(name,value.ToInt());}
		if(type == ValueType.Float){target.Call(name,value.ToFloat());}
		if(type == ValueType.Bool){target.Call(name,value.ToBool());}
	}
}