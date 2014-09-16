using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger (Parameter)")]
public class EventParameterTrigger : ActionPart{
	public string eventName;
	public string parameter;
	public ValueType parameterType;
	public EventFrequency eventFrequency;
	public void OnValidate(){this.DefaultPriority(15);}
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
		if(type == ValueType.String){this.action.owner.Call(name,value);}
		if(type == ValueType.Int){this.action.owner.Call(name,value.ToInt());}
		if(type == ValueType.Float){this.action.owner.Call(name,value.ToFloat());}
		if(type == ValueType.Bool){this.action.owner.Call(name,value.ToBool());}
	}
}