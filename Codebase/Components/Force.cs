using UnityEngine;
using System.Collections;
[RequireComponent(typeof(ColliderController))]
public class Force : MonoBehaviour{
	public Vector3 velocity;
	public Vector3 terminalVelocity = new Vector3(20,20,20);
	public float resistence;
	public float minimumImpactVelocity = 1;
	[HideInInspector] public ColliderController controller;
	public void Awake(){
		Events.Add("OnCollide",(MethodObject)this.OnCollide);
		Events.Add("OnForce",this.AddForce);
		Events.Add("ResetVelocity",this.ResetVelocity);
		this.controller = this.GetComponent<ColliderController>();
	}
	public void Update(){
		if(this.velocity != Vector3.zero){
			Vector3 resistence = this.velocity.Sign() * this.resistence;
			this.velocity -= resistence * Time.deltaTime;
			this.velocity = this.velocity.Clamp(this.terminalVelocity*-1,this.terminalVelocity);
			this.gameObject.Call("OnMove",this.velocity);
			/*this.gameObject.Call("OnMove",new Vector3(this.velocity.x,0,0));
			this.gameObject.Call("OnMove",new Vector3(0,this.velocity.y,0));
			this.gameObject.Call("OnMove",new Vector3(0,0,this.velocity.z));*/
		}
	}
	public void AddForce(Vector3 force){
		if(this.controller.freezePosition[0]){force.x = 0;}
		if(this.controller.freezePosition[1]){force.y = 0;}
		if(this.controller.freezePosition[2]){force.z = 0;}
		if(force != Vector3.zero){
			this.velocity += force;
		}
	}
	public void ResetVelocity(string target){
		if(target.Contains("x")){this.velocity.x = 0;}
		if(target.Contains("y")){this.velocity.y = 0;}
		if(target.Contains("z")){this.velocity.z = 0;}
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