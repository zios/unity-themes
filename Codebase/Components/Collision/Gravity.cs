using Zios;
using UnityEngine;
using System.Collections;
[RequireComponent(typeof(ColliderController))]
[AddComponentMenu("Zios/Component/Physics/Gravity")]
public class Gravity : ActionPart{
	public AttributeVector3 intensity = new Vector3(0,-9.8f,0);
	public AttributeFloat scale = 1.0f;
	public AttributeBool disabled;
	public override void OnValidate(){
		base.OnValidate();
		this.DefaultRate("FixedUpdate");
		this.intensity.Setup("Intensity",this);
		this.disabled.Setup("Disabled",this);
		this.scale.Setup("Scale",this);
	}
	public override void Use(){
		bool blocked = ColliderController.Get(this.gameObject).blocked["down"];
		if(!this.disabled && !blocked){
			Vector3 amount = (this.intensity*this.scale)* Time.fixedDeltaTime;
			this.gameObject.Call("AddForce",amount);
		}
		base.Use();
	}
}
