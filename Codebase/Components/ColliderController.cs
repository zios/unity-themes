using UnityEngine;
using System.Collections;
using System;
[AddComponentMenu("Zios/Component/General/Collider Controller")]
public class ColliderController : MonoBehaviour{
	[HideInInspector]
	public Vector3 move = Vector3.zero;
	void Update(){
		if(this.move != Vector3.zero){
			this.move *= Time.deltaTime;
			RaycastHit hit = new RaycastHit();
			Vector3 start = this.transform.position;
			Vector3 end = start + this.move;
			float distance = Vector3.Distance(start,end);
			this.collider.enabled = false;
			if(this.collider is SphereCollider){
				float radius = ((SphereCollider)this.collider).radius;
				if(Physics.SphereCast(start,radius,this.move.normalized,out hit,distance)){
					if(hit.distance > 0){
						this.move = this.move.normalized * hit.distance;
					}
					else{
						this.move = Vector3.zero;
					}
				}
			}
			else if(this.collider is CapsuleCollider){
				float height = ((CapsuleCollider)this.collider).height;
				float radius = ((CapsuleCollider)this.collider).radius;
				Vector3 center = ((CapsuleCollider)this.collider).center;
				Vector3 rotation = this.transform.rotation * Vector3.up;
				Vector3 p1 = this.transform.position + center + rotation * (-height * 0.5F + radius);
				Vector3 p2 = this.transform.position + center + rotation * (height * 0.5F - radius);
				if(Physics.CapsuleCast(p1,p2,radius,this.move.normalized,out hit,distance)){
					if(hit.distance > 0){
						this.move = this.move.normalized * hit.distance;
					}
					else{
						this.move = Vector3.zero;
					}
				}
			}
			else if(this.collider is BoxCollider){
			}
			this.collider.enabled = true;
			if(this.move != Vector3.zero){
				this.transform.position += this.move;
				this.move = Vector3.zero;
			}
		}
	}
	public void Move(Vector3 move){
		this.move += move;
	}
}
