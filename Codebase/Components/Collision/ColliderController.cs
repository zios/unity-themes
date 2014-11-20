using UnityEngine;
using Zios;
using System.Collections;
using System.Collections.Generic;
using System;
public enum ColliderRate{FixedUpdate,Update};
public enum ColliderMode{Sweep,SweepAndValidate,SweepAndValidatePrecise,Validate};
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
[RequireComponent(typeof(Collider))]
[AddComponentMenu("Zios/Component/Physics/Collider Controller")]
public class ColliderController : MonoBehaviour{
	public static Dictionary<GameObject,ColliderController> instances = new Dictionary<GameObject,ColliderController>();
	public static Collider[] triggers;
	public static bool triggerSetup;
	public static ColliderController Get(GameObject gameObject){
		return ColliderController.instances[gameObject];
	}
	public static bool HasTrigger(Collider collider){
		return Array.IndexOf(ColliderController.triggers,collider) != -1;
	}
	public ColliderMode mode;
	public ColliderRate rate;
	public List<Vector3> move = new List<Vector3>();
	public List<Vector3> moveRaw = new List<Vector3>();
	[NonSerialized] public Vector3 lastDirection;
	public Dictionary<string,bool> blocked = new Dictionary<string,bool>();
	public Dictionary<string,float> lastBlockedTime = new Dictionary<string,float>();
	public AttributeFloat fixedTimestep = 0.002f;
	public AttributeFloat maxStepHeight = 0;
	public AttributeFloat maxSlopeAngle = 70;
	public AttributeFloat minSlideAngle = 50;
	public AttributeFloat hoverDistance = 0.02f;
	private Vector3 lastPosition;
	//--------------------------------
	// Unity-Specific
	//--------------------------------
	public void Reset(){this.Awake();}
	public void OnApplicationQuit(){this.Awake();}
	public void Awake(){
		this.ResetBlocked(true);
		this.fixedTimestep.Setup("Fixed Timestep",this);
		this.maxStepHeight.Setup("Max Step Height",this);
		this.maxSlopeAngle.Setup("Max Slope Angle",this);
		this.minSlideAngle.Setup("Min Slide Angle",this);
		this.hoverDistance.Setup("Hover Distance",this);
		Events.Add("Add Move",(MethodVector3)this.AddMove);
		Events.Add("Add Move Raw",(MethodVector3)this.AddMoveRaw);
		Events.Register("Trigger",this.gameObject);
		Events.Register("Collide",this.gameObject);
		if(Application.isPlaying){
			if(this.rigidbody == null){
				this.gameObject.AddComponent("Rigidbody");
			}
			this.rigidbody.useGravity = false;
			this.rigidbody.freezeRotation = this.mode != ColliderMode.Sweep;
			this.rigidbody.isKinematic = this.mode == ColliderMode.Sweep;
			this.CheckActive("Sleep");
			ColliderController.instances[this.gameObject] = this;
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
			Time.fixedDeltaTime = this.fixedTimestep;
		}
	}
	public void OnDrawGizmosSelected(){
		Gizmos.color = Color.white;
		Vector3 raise = this.transform.up * this.maxStepHeight;
		Vector3 start = this.transform.position;
		Vector3 end = this.transform.position+(this.transform.forward*2);
		Gizmos.DrawLine(start+raise,end+raise);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(start,start+(-Vector3.up*0.5f));
	}
	public void FixedUpdate(){
		if(this.rate == ColliderRate.FixedUpdate){
			this.Step();
		}
	}
	public void Update(){
		if(this.rate == ColliderRate.Update){
			this.Step();
		}
	}
	//--------------------------------
	// Internal
	//--------------------------------
	private void Step(){
		if(!Application.isPlaying){return;}
		bool positionAltered = this.lastPosition != this.transform.position;
		if(this.move.Count > 0 || this.moveRaw.Count > 0 || positionAltered){
			this.ResetBlocked();
			this.CheckActive("WakeUp");
			Vector3 cumulative = Vector3.zero;
			Vector3 initial = this.rigidbody.position;
			foreach(Vector3 current in this.move){
				cumulative += current;
				float time = this.rate == ColliderRate.Update ? Time.deltaTime : Time.fixedDeltaTime;
				Vector3 move = this.NullBlocked(current) * time;
				this.StepMove(current,move);
			}
			foreach(Vector3 current in this.moveRaw){
				cumulative += current;
				Vector3 move = this.NullBlocked(current);
				this.StepMove(current,move);
			}
			if(this.mode == ColliderMode.SweepAndValidate){
				Vector3 totalMove = this.rigidbody.position-initial;
				this.rigidbody.position = initial;
				this.rigidbody.MovePosition(initial + totalMove);
			}
			this.lastDirection = cumulative.normalized;
			if(this.move.Count > 0){this.move = new List<Vector3>();}
			if(this.moveRaw.Count > 0){this.moveRaw = new List<Vector3>();}
			this.CheckActive("Sleep");
			if(this.mode == ColliderMode.Sweep){
				this.transform.position = this.rigidbody.position;				
			}
			this.lastPosition = this.transform.position;
		}
		if(this.mode != ColliderMode.Sweep){
			this.rigidbody.velocity *= 0;
			this.rigidbody.angularVelocity *= 0;
		}
		this.CheckBlocked();
	}
	private void StepMove(Vector3 current,Vector3 move){
		if(this.mode == ColliderMode.Validate){
			this.rigidbody.MovePosition(this.rigidbody.position + move);
			return;
		}
		if(move == Vector3.zero){return;}
		RaycastHit hit;
		Vector3 startPosition = this.rigidbody.position;
		Vector3 direction = move.normalized;
		float distance = Vector3.Distance(startPosition,startPosition+move) + this.hoverDistance*2;
		bool contact = this.rigidbody.SweepTest(direction,out hit,distance);
		bool isTrigger = ColliderController.HasTrigger(hit.collider);
		if(this.CheckSlope(current)){return;}
		if(contact && this.CheckSlope(current,hit)){return;}
		if(contact){
			if(this.CheckStep(current)){return;}
			if(isTrigger){
				hit.transform.gameObject.Call("Trigger",this.collider);
				return;
			}
			this.SetPosition(this.rigidbody.position + (direction * (hit.distance-this.hoverDistance*2)));
			CollisionData otherCollision = new CollisionData(this,this.gameObject,-direction,distance,false);
			CollisionData selfCollision = new CollisionData(this,hit.transform.gameObject,direction,distance,true);
			if(direction.z > 0){this.blocked["forward"] = true;}
			if(direction.z < 0){this.blocked["back"] = true;}
			if(direction.y > 0){this.blocked["up"] = true;}
			if(direction.y < 0){this.blocked["down"] = true;}
			if(direction.x > 0){this.blocked["right"] = true;}
			if(direction.x < 0){this.blocked["left"] = true;}
			hit.transform.gameObject.Call("Collide",otherCollision);
			this.gameObject.Call("Collide",selfCollision);
		}
		else{
			this.SetPosition(startPosition + move);
		}
	}
	private bool CheckSlope(Vector3 current,RaycastHit slopeHit){
		if(this.maxSlopeAngle != 0 || this.minSlideAngle != 0){
			Vector3 motion = new Vector3(current.x,0,current.z);
			float angle = Mathf.Acos(Mathf.Clamp(slopeHit.normal.y,-1,1)) * 90;
			bool yOnly = motion == Vector3.zero;
			bool isSlope = angle > 0;
			if(isSlope){
				bool slideCheck = yOnly && (angle > this.minSlideAngle);
				bool slopeCheck = !yOnly && (angle < this.maxSlopeAngle);
				if(slopeCheck || slideCheck){
					Vector3 cross = Vector3.Cross(slopeHit.normal,current);
					Vector3 change = Vector3.Cross(cross,slopeHit.normal) * Time.fixedDeltaTime;
					this.SetPosition(this.rigidbody.position + change);
					this.blocked["down"] = false;
					return true;
				}
			}
		}
		return false;
	}
	private bool CheckSlope(Vector3 current){
		if(this.maxSlopeAngle != 0 || this.minSlideAngle != 0){
			RaycastHit slopeHit;
			bool slopeTest = this.rigidbody.SweepTest(-Vector3.up,out slopeHit,0.5f);
			if(slopeTest){
				return this.CheckSlope(current,slopeHit);
			}
		}
		return false;
	}
	private bool CheckStep(Vector3 current){
		if(this.maxStepHeight != 0 && current.y == 0){
			RaycastHit stepHit;
			Vector3 move = this.NullBlocked(current) * Time.fixedDeltaTime;
			Vector3 direction = move.normalized;
			Vector3 position = this.rigidbody.position;
			float distance = Vector3.Distance(position,position+move) + this.hoverDistance*2;
			this.SetPosition(this.rigidbody.position + (Vector3.up * this.maxStepHeight));
			bool stepTest = this.rigidbody.SweepTest(direction,out stepHit,distance);
			if(!stepTest){
				this.SetPosition(this.rigidbody.position + move);
				this.rigidbody.SweepTest(-Vector3.up,out stepHit);
				this.SetPosition(this.rigidbody.position + (-Vector3.up*(stepHit.distance-0.01f)));
				this.blocked["down"] = true;
				return true;
			}
			this.SetPosition(position);
		}
		return false;
	}
	private void SetPosition(Vector3 position){
		if(this.mode != ColliderMode.SweepAndValidatePrecise){
			this.rigidbody.position = position;
			return;
		}
		this.rigidbody.MovePosition(position);
	}
	private void CheckActive(string state){
		if(this.mode == ColliderMode.Sweep){
			if(state == "Sleep"){this.rigidbody.Sleep();}
		}
		if(state == "Wake"){this.rigidbody.WakeUp();}
	}
	private void CheckBlocked(){
		foreach(var item in this.blocked){
			if(this.blocked[item.Key]){
				this.lastBlockedTime[item.Key] = Time.time;
			}
		}
	}
	private Vector3 NullBlocked(Vector3 move){
		if(this.blocked["left"] && move.x < 0){move.x = 0;}
		if(this.blocked["right"] && move.x > 0){move.x = 0;}
		if(this.blocked["up"] && move.y > 0){move.y = 0;}
		if(this.blocked["down"] && move.y < 0){move.y = 0;}
		if(this.blocked["forward"] && move.z > 0){move.z = 0;}
		if(this.blocked["back"] && move.z < 0){move.z = 0;}
		return move;
	}
	private void ResetBlocked(bool clearTime=false){
		string[] names = new string[]{"forward","back","up","down","right","left"};
		foreach(string name in names){
			this.blocked[name] = false;
			if(clearTime){this.lastBlockedTime[name] = 0;}
		}
	}
	public void AddMove(Vector3 move){
		if(!this.enabled){return;}
		if(move != Vector3.zero){
			this.move.Add(move);
		}
	}
	public void AddMoveRaw(Vector3 move){
		if(!this.enabled){return;}
		if(move != Vector3.zero){
			this.moveRaw.Add(move);
		}
	}
}
