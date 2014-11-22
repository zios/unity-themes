using Zios;
using UnityEngine;
using System.Collections;
[RequireComponent(typeof(ColliderController))]
[AddComponentMenu("Zios/Component/Physics/Gravity")]
public class Gravity : ActionPart{
	public AttributeVector3 intensity = new Vector3(0,-9.8f,0);
	public AttributeFloat scale = 1.0f;
	public AttributeBool disabled = false;
	public override void Awake(){
		base.Awake();
		this.DefaultRate("FixedUpdate");
		this.intensity.Setup("Intensity",this);
		this.disabled.Setup("Disabled",this);
		this.scale.Setup("Scale",this);
	}
	public override void Use(){
		if(!this.disabled){
			Vector3 amount = (this.intensity*this.scale)* Time.fixedDeltaTime;
			this.gameObject.Call("Add Force",amount);
		}
		base.Use();
	}
}
