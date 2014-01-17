using UnityEngine;
using System.Collections;
[RequireComponent(typeof(ColliderController))]
public class Force : MonoBehaviour{
	public Vector3 velocity;
	[HideInInspector] public ColliderController controller;
	public float resistence;
	public float minimumImpactVelocity = 1;
	public void Awake(){
		Events.Add("OnCollide",(MethodObject)this.OnCollide);
		Events.Add("OnForce",this.AddForce);
		this.controller = this.GetComponent<ColliderController>();
	}
	public void Update(){
		if(this.velocity != Vector3.zero){
			Vector3 resistence = this.velocity.Sign() * (this.resistence * Time.deltaTime);
			this.velocity -= resistence;
			this.gameObject.Call("OnMove",this.velocity);
		}
	}
	public void AddForce(Vector3 force){
		if(force != Vector3.zero){
			this.velocity += force;
		}
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
					this.gameObject.Call("OnImpact",impact);
				}
			}
		}
	}
}