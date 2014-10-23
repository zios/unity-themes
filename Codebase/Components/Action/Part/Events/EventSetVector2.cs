using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Set (Vector2)")]
public class EventSetVector2 : EventSet{
	public Vector2 parameter;
	public bool scaleByIntensity;
	public override void Use(){
		Vector2 value = this.parameter;
		if(this.scaleByIntensity){value *= this.action.intensity;}
		this.target.Set(value);
		base.Use();
	}
}
