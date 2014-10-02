using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger (Parameter)")]
public class EventParameterTrigger : EventTrigger{
	public string parameter;
	public ValueType parameterType;
	public override void CallEvent(){
		string name = this.eventName;
		string value = this.parameter;
		ValueType type = this.parameterType;
		GameObject target = this.eventTarget;
		if(type == ValueType.String){target.Call(name,value);}
		if(type == ValueType.Int){target.Call(name,value.ToInt());}
		if(type == ValueType.Float){target.Call(name,value.ToFloat());}
		if(type == ValueType.Bool){target.Call(name,value.ToBool());}
	}
}