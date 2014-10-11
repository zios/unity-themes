using Zios;
using UnityEngine;
public enum ValueType{String,Int,Float,Bool}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Event Trigger (Parameter)")]
public class EventParameterTrigger : EventTrigger{
	public string parameter;
	public ValueType parameterType;
	public override void Use(){
		string name = this.eventTarget.setEvent;
		string value = this.parameter;
		ValueType type = this.parameterType;
		EventTarget target = this.eventTarget;
		if(type == ValueType.String){target.Set(name,value);}
		if(type == ValueType.Int){target.Set(name,value.ToInt());}
		if(type == ValueType.Float){target.Set(name,value.ToFloat());}
		if(type == ValueType.Bool){target.Set(name,value.ToBool());}
		base.Use();
	}
}