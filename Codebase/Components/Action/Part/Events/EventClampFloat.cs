using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Clamp (Vector3)")]
public class ClampFloat : ActionPart{
	public EventManageTarget target = new EventManageTarget();
	public float minimum;
	public float maximum;
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
	}
	public override void Use(){
		float value = (float)this.target.Get();
		bool exists = this.minimum != 0 && this.maximum != 0;
		if(exists && value < this.minimum){value = this.minimum;}
		if(exists && value > this.maximum){value = this.maximum;}
		this.target.Set(value);
		base.Use();
	}
}