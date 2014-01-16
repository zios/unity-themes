using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class CollisionData{
	public bool isSource;
	public ColliderController sourceController;
	public GameObject gameObject;
	public Vector3 direction;
	public float force;
	public CollisionData(ColliderController controller,GameObject gameObject,Vector3 direction,float force,bool isSource){
		this.sourceController = controller;
		this.gameObject = gameObject;
		this.direction = direction;
		this.force = force;
		this.isSource = isSource;
	}
}
[AddComponentMenu("Zios/Component/General/Collider Controller")]
public class ColliderController : MonoBehaviour{
	public List<Vector3> move = new List<Vector3>();
	[HideInInspector] public Dictionary<string,bool> blocked = new Dictionary<string,bool>();
	public float hoverWidth = 0.05f;
	public float skinWidth = 0.05f;
	public bool persistentBlockChecks;
	public void Awake(){
		Events.Add("OnMove",this.Move);
		this.ResetBlocked();
	}
	public void OnEnable(){
		if(this.rigidbody == null){
			this.gameObject.AddComponent("Rigidbody");
		}
		this.rigidbody.Sleep();
	}
	public void Update(){
		this.UpdatePosition();
	}
	public void ResetBlocked(){
		this.blocked["forward"] = false;
		this.blocked["back"] = false;
		this.blocked["up"] = false;
		this.blocked["down"] = false;
		this.blocked["right"] = false;
		this.blocked["left"] = false;
	}
	public void CheckBlocked(){
		if(this.persistentBlockChecks){
			this.rigidbody.WakeUp();
			RaycastHit hit;
			float distance = this.skinWidth + this.hoverWidth + 0.01f;
			this.blocked["forward"] = this.rigidbody.SweepTest(this.transform.forward,out hit,distance);
			this.blocked["back"] = this.rigidbody.SweepTest(-this.transform.forward,out hit,distance);
			this.blocked["up"] = this.rigidbody.SweepTest(this.transform.up,out hit,distance);
			this.blocked["down"] = this.rigidbody.SweepTest(-this.transform.up,out hit,distance);
			this.blocked["right"] = this.rigidbody.SweepTest(this.transform.right,out hit,distance);
			this.blocked["left"] = this.rigidbody.SweepTest(-this.transform.right,out hit,distance);
			this.rigidbody.Sleep();
		}
	}
	public void Move(Vector3 move){
		if(move != Vector3.zero){
			this.move.Add(move);
		}
	}
	public void UpdatePosition(){
		if(this.move.Count > 0){
			this.ResetBlocked();
			this.rigidbody.WakeUp();
			foreach(Vector3 current in this.move){
				RaycastHit hit;
				Vector3 move = current * Time.deltaTime;
				Vector3 direction = move.normalized;
				if(this.rigidbody.SweepTest(direction,out hit,move.magnitude)){
					float hitDistance = Mathf.Clamp(hit.distance,this.skinWidth,1);
					move = direction * (hitDistance - this.skinWidth);
					CollisionData otherCollision = new CollisionData(this,this.gameObject,-direction,move.magnitude,false);
					CollisionData selfCollision = new CollisionData(this,hit.transform.gameObject,direction,move.magnitude,true);
					if(direction.z > 0){this.blocked["forward"] = true;}
					if(direction.z < 0){this.blocked["back"] = true;}
					if(direction.y > 0){this.blocked["up"] = true;}
					if(direction.y < 0){this.blocked["down"] = true;}
					if(direction.x > 0){this.blocked["right"] = true;}
					if(direction.x < 0){this.blocked["left"] = true;}
					hit.transform.gameObject.Call("OnCollide",otherCollision);
					this.gameObject.Call("OnCollide",selfCollision);
				}
				this.transform.position += move;
			}
			this.move.Clear();
			this.rigidbody.Sleep();
		}
		this.CheckBlocked();
	}
}
