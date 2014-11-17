using Zios;
using UnityEngine;
using System;
using System.Collections;
[RequireComponent(typeof(ColliderController))]
[AddComponentMenu("Zios/Component/Physics/Force")]
public class Force : ActionPart{
	public AttributeVector3 velocity = Vector3.zero;
	public AttributeVector3 terminalVelocity = new Vector3(20,20,20);
	public AttributeVector3 resistence = new Vector3(8,0,8);
	public AttributeFloat minimumImpactVelocity = 1;
	public AttributeBool disabled = false;
	[NonSerialized] public ColliderController controller;
	public override void Awake(){
		base.Awake();
		this.DefaultRate("FixedUpdate");
		this.velocity.Setup("Velocity",this);
		this.terminalVelocity.Setup("Terminal Velocity",this);
		this.resistence.Setup("Resistence",this);
		this.minimumImpactVelocity.Setup("Minimum Impact Velocity",this);
		this.disabled.Setup("Disabled",this);
		Events.Register("On Impact",this.gameObject);
		Events.Register("Add Move",this.gameObject);
		Events.Add("Collide",(MethodObject)this.OnCollide);
		this.controller = this.GetComponent<ColliderController>();
	}
	public override void Use(){
		if(!this.disabled && this.velocity != Vector3.zero){
			Vector3 resistence = Vector3.Scale(this.velocity.Sign(),this.resistence);
			this.velocity -= resistence * Time.fixedDeltaTime;
			this.velocity = this.velocity.Clamp(this.terminalVelocity.Get()*-1,this.terminalVelocity);
			this.gameObject.Call("Add Move",new Vector3(this.velocity.x,0,0));
			this.gameObject.Call("Add Move",new Vector3(0,this.velocity.y,0));
			this.gameObject.Call("Add Move",new Vector3(0,0,this.velocity.z));
		}
		base.Use();
	}
	public void OnCollide(object collision){
		CollisionData data = (CollisionData)collision;
		if(data.isSource){
			Vector3 original = this.velocity;
			if(data.sourceController.blocked["down"] && this.velocity.y < 0){this.velocity.y = 0;}
			if(data.sourceController.blocked["up"] && this.velocity.y > 0){this.velocity.y = 0;}
			if(data.sourceController.blocked["right"] && this.velocity.x > 0){this.velocity.x = 0;}
			if(data.sourceController.blocked["left"] && this.velocity.x < 0){this.velocity.x = 0;}
			if(data.sourceController.blocked["forward"] && this.velocity.z > 0){this.velocity.z = 0;}
			if(data.sourceController.blocked["back"] && this.velocity.z < 0){this.velocity.z = 0;}
			if(original != this.velocity){
				Vector3 impact = (this.velocity - original);
				float impactStrength = impact.magnitude;
				if(impactStrength > this.minimumImpactVelocity){
					this.gameObject.Call("On Impact",impact);
				}
			}
		}
	}
}