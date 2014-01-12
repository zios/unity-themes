using UnityEngine;
using System.Collections;
using System;
[AddComponentMenu("Zios/Component/General/Collider Controller")]
public class ColliderController : MonoBehaviour{
	[HideInInspector]
	public Vector3 move = Vector3.zero;
	public void OnEnable(){
		if(this.rigidbody == null){
			this.gameObject.AddComponent("Rigidbody");
		}
		this.rigidbody.Sleep();
	}
	void FixedUpdate(){
		if(this.move != Vector3.zero){
			this.rigidbody.WakeUp();
			this.move *= Time.deltaTime;
			RaycastHit hit = new RaycastHit();
			Vector3 start = this.transform.position;
			Vector3 end = start + this.move;
			float distance = Vector3.Distance(start,end);
			if(this.rigidbody.SweepTest(this.move.normalized,out hit,distance)){
				if(hit.distance > 0){
					this.move = this.move.normalized * hit.distance;
				}
				else{
					this.move = Vector3.zero;
				}
			}
			if(this.move != Vector3.zero){
				this.transform.position += this.move;
				this.move = Vector3.zero;
			}
			this.rigidbody.Sleep();
		}
	}
	public void Move(Vector3 move){
		this.move += move;
	}
}
