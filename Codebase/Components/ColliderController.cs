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
	static public Collider[] triggers;
	static public bool triggerSetup;
	[HideInInspector] public List<Vector3> move = new List<Vector3>();
	public Dictionary<string,bool> blocked = new Dictionary<string,bool>();
	public Dictionary<string,float> lastBlockedTime = new Dictionary<string,float>();
	public bool[] freezePosition = new bool[3]{false,false,false};
	public float hoverWidth = 0.0001f;
	public float skinWidth = 0.0001f;
	public bool persistentBlockChecks;
	public void Awake(){
		Events.Add("OnMove",this.Move);
		this.ResetBlocked(true);
	}
	public void Start(){
		if(!ColliderController.triggerSetup){
			Collider[] colliders = (Collider[])Resources.FindObjectsOfTypeAll(typeof(Collider));
			List<Collider> triggers = new List<Collider>();
			foreach(Collider collider in colliders){
				if(collider.isTrigger){
					collider.isTrigger = false;
					triggers.Add(collider);
				}
			}
			ColliderController.triggers = triggers.ToArray();
			ColliderController.triggerSetup = true;
		}
	}
	public void OnEnable(){
		if(this.rigidbody == null){
			this.gameObject.AddComponent("Rigidbody");
		}	
		this.rigidbody.isKinematic = true;
		this.rigidbody.Sleep();
	}
	public void Update(){
		this.UpdatePosition();
	}
	public void ResetBlocked(bool clearTime=false){
		string[] names = new string[]{"forward","back","up","down","right","left"};
		foreach(string name in names){
			this.blocked[name] = false;
			if(clearTime){this.lastBlockedTime[name] = 0;}
		}
	}
	public float GetUnblockedDuration(string name){
		return Time.time - this.lastBlockedTime[name];
	}
	public void CheckBlocked(){
		if(this.persistentBlockChecks){
			RaycastHit hit;
			this.rigidbody.WakeUp();
			float distance = this.skinWidth + this.hoverWidth + 0.01f;
			this.blocked["forward"] = this.rigidbody.SweepTest(this.transform.forward,out hit,distance);
			this.blocked["back"] = this.rigidbody.SweepTest(-this.transform.forward,out hit,distance);
			this.blocked["up"] = this.rigidbody.SweepTest(this.transform.up,out hit,distance);
			this.blocked["down"] = this.rigidbody.SweepTest(-this.transform.up,out hit,distance);
			this.blocked["right"] = this.rigidbody.SweepTest(this.transform.right,out hit,distance);
			this.blocked["left"] = this.rigidbody.SweepTest(-this.transform.right,out hit,distance);
			this.rigidbody.Sleep();
		}
		foreach(var item in this.blocked){
			if(this.blocked[item.Key]){
				this.lastBlockedTime[item.Key] = Time.time;
			}
		}
	}
	public void Move(Vector3 move){
		if(move != Vector3.zero){
			if(this.freezePosition[0]){move.x = 0;}
			if(this.freezePosition[1]){move.y = 0;}
			if(this.freezePosition[2]){move.z = 0;}
			this.move.Add(move);
		}
	}
	public void UpdatePosition(){
		if(this.move.Count > 0){
			this.ResetBlocked();
			this.rigidbody.WakeUp();
			foreach(Vector3 current in this.move){
				Vector3 move = current * Time.deltaTime;
				Vector3 direction = move.normalized;
				RaycastHit hit;
				if(this.rigidbody.SweepTest(direction,out hit,move.magnitude+this.hoverWidth)){
					move = direction * (hit.distance - this.skinWidth - this.hoverWidth);
					CollisionData otherCollision = new CollisionData(this,this.gameObject,-direction,move.magnitude,false);
					CollisionData selfCollision = new CollisionData(this,hit.transform.gameObject,direction,move.magnitude,true);
					if(direction.z > 0){this.blocked["forward"] = true;}
					if(direction.z < 0){this.blocked["back"] = true;}
					if(direction.y > 0){this.blocked["up"] = true;}
					if(direction.y < 0){this.blocked["down"] = true;}
					if(direction.x > 0){this.blocked["right"] = true;}
					if(direction.x < 0){this.blocked["left"] = true;}
					bool isTrigger = Array.IndexOf(ColliderController.triggers,hit.collider) != -1;
					if(isTrigger){
						hit.transform.gameObject.Call("OnTrigger",this.collider);
						continue;
					}  
					hit.transform.gameObject.Call("OnCollide",otherCollision);
					this.gameObject.Call("OnCollide",selfCollision);
				}
				this.transform.position += move;
			}
			this.move.Clear();
			this.rigidbody.Sleep();
		}
		this.CheckBlocked();
		this.Freeze();
	}
	public void Freeze(){
		Vector3 position = this.transform.position;
		if(this.freezePosition[0]){position.x = 0;}
		if(this.freezePosition[1]){position.y = 0;}
		if(this.freezePosition[2]){position.z = 0;}
		this.transform.position = position;
	}
}
