using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Motion{
	using Attributes;
	using Events;
	public enum ColliderMode{Sweep,SweepAndValidate};
	public class CollisionData{
		public bool isSource;
		public bool isTrigger;
		public CollisionData otherData;
		public ColliderController sourceController;
		public GameObject hitObject;
		public Vector3 direction;
		public float force;
		public float endTime;
		public CollisionData(ColliderController source,GameObject hit,Vector3 direction,float force,bool isSource,bool isTrigger){
			this.sourceController = source;
			this.hitObject = hit;
			this.direction = direction;
			this.force = force;
			this.isSource = isSource;
			this.isTrigger = isTrigger;
		}
	}
	[Serializable]
	public class BlockedDirection{
		public AttributeBool forward = false;
		public AttributeBool back = false;
		public AttributeBool up = false;
		public AttributeBool down = false;
		public AttributeBool right = false;
		public AttributeBool left = false;
	}
	[Serializable]
	public class BlockedTime{
		public AttributeFloat forward = 0;
		public AttributeFloat back = 0;
		public AttributeFloat up = 0;
		public AttributeFloat down = 0;
		public AttributeFloat right = 0;
		public AttributeFloat left = 0;
	}
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("Zios/Component/Motion/Collider Controller")]
	public class ColliderController : ManagedMonoBehaviour{
		private Dictionary<GameObject,CollisionData> collisions = new Dictionary<GameObject,CollisionData>();
		private Dictionary<GameObject,CollisionData> frameCollisions = new Dictionary<GameObject,CollisionData>();
		private Vector3 lastPosition;
		[Advanced] public ColliderMode mode;
		[NonSerialized] public List<Vector3> move = new List<Vector3>();
		[NonSerialized] public List<Vector3> moveRaw = new List<Vector3>();
		[NonSerialized] public Vector3 lastDirection;
		public bool persistentBlockChecks;
		public AttributeFloat fixedTimestep = 0.002f;
		public AttributeFloat maxStepHeight = 0;
		public AttributeFloat maxSlopeAngle = 70;
		public AttributeFloat minSlideAngle = 50;
		public AttributeFloat hoverDistance = 0.02f;
		public AttributeFloat collisionPersist = 0.2f;
		[Internal] public bool hasTriggers;
		[Internal] public AttributeBool onSlope = false;
		[Internal] public AttributeBool onSlide = false;
		[Internal] public AttributeVector3 slopeNormal = Vector3.zero;
		[Internal] public BlockedDirection blocked = new BlockedDirection();
		[Internal] public BlockedTime lastBlockedTime = new BlockedTime();
		//================================
		// Static
		//================================
		private static Dictionary<GameObject,ColliderController> instances = new Dictionary<GameObject,ColliderController>();
		public static void SetupColliders(){
			foreach(var collider in Locate.GetSceneComponents<Collider>()){
				Event.Register("On Collision",collider.gameObject);
				Event.Register("On Collision Start",collider.gameObject);
				Event.Register("On Collision End",collider.gameObject);
			}
			Event.Add("On Scene Loaded",ColliderController.SetupColliders).SetPermanent();
			Event.Add("On Hierarchy Changed",ColliderController.SetupColliders).SetPermanent();
		}
		//================================
		// Unity-Specific
		//================================
		public override void Awake(){
			base.Awake();
			this.blocked.forward.Setup("Blocked/Forward",this);
			this.blocked.back.Setup("Blocked/Back",this);
			this.blocked.up.Setup("Blocked/Up",this);
			this.blocked.down.Setup("Blocked/Down",this);
			this.blocked.right.Setup("Blocked/Right",this);
			this.blocked.left.Setup("Blocked/Left",this);
			this.lastBlockedTime.forward.Setup("Last Blocked Time/Forward",this);
			this.lastBlockedTime.back.Setup("Last Blocked Time/Back",this);
			this.lastBlockedTime.up.Setup("Last Blocked Time/Up",this);
			this.lastBlockedTime.down.Setup("Last Blocked Time/Down",this);
			this.lastBlockedTime.right.Setup("Last Blocked Time/Right",this);
			this.lastBlockedTime.left.Setup("Last Blocked Time/Left",this);
			this.fixedTimestep.Setup("Fixed Timestep",this);
			this.maxStepHeight.Setup("Max Step Height",this);
			this.maxSlopeAngle.Setup("Max Slope Angle",this);
			this.minSlideAngle.Setup("Min Slide Angle",this);
			this.hoverDistance.Setup("Hover Distance",this);
			this.collisionPersist.Setup("Collision Persist",this);
			this.onSlope.Setup("On Slope",this);
			this.onSlide.Setup("On Slide",this);
			this.slopeNormal.Setup("Slope Normal",this);
			this.ResetBlocked(true);
			Event.Add("Add Move",(MethodVector3)this.AddMove,this.gameObject);
			Event.Add("Add Move Raw",(MethodVector3)this.AddMoveRaw,this.gameObject);
			Event.Add("On Collision",(MethodObject)this.OnCollision,this.gameObject);
			Event.Add("On Collision Start",(MethodObject)this.OnCollisionStart,this.gameObject);
			Event.Add("On Collision End",(MethodObject)this.OnCollisionEnd,this.gameObject);
			ColliderController.SetupColliders();
			if(Application.isPlaying){
				var body = this.gameObject.AddComponent<Rigidbody>();
				body.useGravity = false;
				body.freezeRotation = true;
				body.isKinematic = false;
				this.CheckActive("Sleep");
				ColliderController.instances[this.gameObject] = this;
				Time.fixedDeltaTime = this.fixedTimestep;
			}
		}
		public void OnDrawGizmosSelected(){
			if(!Attribute.ready){return;}
			Gizmos.color = Color.white;
			Vector3 raise = this.transform.up * this.maxStepHeight;
			Vector3 start = this.transform.position;
			Vector3 end = this.transform.position+(this.transform.forward*2);
			Gizmos.DrawLine(start+raise,end+raise);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(start,start+(-Vector3.up*0.5f));
		}
		//================================
		// Internal
		//================================
		public override void Step(){
			var rigidbody = this.GetComponent<Rigidbody>();
			bool positionAltered = this.lastPosition != this.transform.position;
			if(this.move.Count > 0 || this.moveRaw.Count > 0 || positionAltered){
				this.ResetBlocked();
				this.CheckActive("WakeUp");
				Vector3 cumulative = Vector3.zero;
				Vector3 initial = rigidbody.position;
				foreach(Vector3 current in this.move){
					cumulative += current;
					Vector3 move = this.NullBlocked(current) * this.GetTimeOffset();
					this.StepMove(current,move);
				}
				foreach(Vector3 current in this.moveRaw){
					cumulative += current;
					Vector3 move = this.NullBlocked(current);
					this.StepMove(current,move);
				}
				if(this.mode == ColliderMode.SweepAndValidate){
					Vector3 totalMove = rigidbody.position-initial;
					rigidbody.position = initial;
					rigidbody.MovePosition(initial + totalMove);
				}
				this.lastDirection = cumulative.normalized;
				if(this.move.Count > 0){this.move = new List<Vector3>();}
				if(this.moveRaw.Count > 0){this.moveRaw = new List<Vector3>();}
				this.CheckActive("Sleep");
				this.transform.position = rigidbody.position;
				this.lastPosition = this.transform.position;
			}
			rigidbody.velocity *= 0;
			rigidbody.angularVelocity *= 0;
			this.CheckCollisionEnd();
			this.CheckCollisionStart();
			this.CheckBlocked();
			this.frameCollisions.Clear();
		}
		private void CheckCollisionEnd(){
			var rigidbody = this.GetComponent<Rigidbody>();
			if(this.hasTriggers){
				foreach(var current in Physics.OverlapSphere(rigidbody.position+rigidbody.centerOfMass,0.5f).Where(x=>x.isTrigger).Select(x=>x.gameObject)){
					if(current == this.gameObject){continue;}
					if(!this.frameCollisions.ContainsKey(current) && this.collisions.ContainsKey(current)){
						var collision = this.collisions[current];
						this.frameCollisions[current] = collision;
						collision.hitObject.CallEvent("On Collision",collision.otherData);
						this.gameObject.CallEvent("On Collision",collision);
					}
				}
			}
			var cleanup = new List<Action>();
			foreach(var collision in this.collisions){
				var target = collision.Key;
				if(!this.frameCollisions.ContainsKey(target)){
					var collisionData = collision.Value;
					if(Time.time > collisionData.endTime){
						Action method = ()=>{
							target.CallEvent("On Collision End",collisionData.otherData);
							this.gameObject.CallEvent("On Collision End",collisionData);
						};
						cleanup.Add(method);
					}
				}
			}
			cleanup.ForEach(x=>x());
		}
		private void CheckCollisionStart(){
			foreach(var collision in this.frameCollisions){
				var target = collision.Key;
				if(!this.collisions.ContainsKey(target)){
					var collisionData = collision.Value;
					target.CallEvent("On Collision Start",collisionData.otherData);
					this.gameObject.CallEvent("On Collision Start",collisionData);
				}
				this.collisions[collision.Key].endTime = Time.time + this.collisionPersist;
			}
		}
		private void StepMove(Vector3 current,Vector3 move){
			if(move == Vector3.zero){return;}
			var rigidbody = this.GetComponent<Rigidbody>();
			RaycastHit hit;
			Vector3 startPosition = rigidbody.position;
			Vector3 direction = move.normalized;
			float distance = Vector3.Distance(startPosition,startPosition+move) + this.hoverDistance*2;
			bool contact = rigidbody.SweepTest(direction,out hit,distance);
			bool isTrigger = !hit.collider.IsNull() ? hit.collider.isTrigger : false;
			if(!contact || isTrigger){
				this.SetPosition(startPosition + move);
				if(this.CheckStepDown()){return;}
			}
			else{
				if(this.CheckSlopeUnder(current)){return;}
				if(this.CheckSlope(current,hit)){return;}
				if(this.CheckStepUp(current)){return;}
				this.SetPosition(rigidbody.position + (direction * (hit.distance-this.hoverDistance*2)));
				if(direction.z > 0){this.blocked.forward.Set(true);}
				if(direction.z < 0){this.blocked.back.Set(true);}
				if(direction.y > 0){this.blocked.up.Set(true);}
				if(direction.y < 0){this.blocked.down.Set(true);}
				if(direction.x > 0){this.blocked.right.Set(true);}
				if(direction.x < 0){this.blocked.left.Set(true);}
			}
			if(contact){
				GameObject hitObject = hit.transform.gameObject;
				CollisionData otherData = new CollisionData(this,hitObject,-direction,distance,false,isTrigger);
				CollisionData selfData = new CollisionData(this,hitObject,direction,distance,true,isTrigger);
				otherData.otherData = selfData;
				selfData.otherData = otherData;
				hitObject.CallEvent("On Collision",otherData);
				this.gameObject.CallEvent("On Collision",selfData);
			}
		}
		private bool CheckSlope(Vector3 current,RaycastHit slopeHit){
			if(this.maxSlopeAngle != 0 || this.minSlideAngle != 0){
				var rigidbody = this.GetComponent<Rigidbody>();
				Vector3 motion = new Vector3(current.x,0,current.z);
				float angle = Mathf.Acos(Mathf.Clamp(slopeHit.normal.y,-1,1)) * 90;
				bool yOnly = motion == Vector3.zero;
				bool isSlope = angle > 0;
				if(yOnly){
					this.onSlope.Set(false);
					this.onSlide.Set(false);
				}
				if(isSlope){
					bool slideCheck = yOnly && (angle > this.minSlideAngle);
					bool slopeCheck = !yOnly && (angle < this.maxSlopeAngle);
					if(yOnly){
						this.onSlope.Set(true);
						this.onSlide.Set(slideCheck);
					}
					if(slopeCheck || slideCheck){
						Vector3 cross = Vector3.Cross(slopeHit.normal,current);
						Vector3 change = Vector3.Cross(cross,slopeHit.normal) * this.GetTimeOffset();
						this.SetPosition(rigidbody.position + change);
						this.blocked.down.Set(false);
						this.slopeNormal.Set(slopeHit.normal);
						return true;
					}
				}
			}
			return false;
		}
		private bool CheckSlopeUnder(Vector3 current){
			if(this.maxSlopeAngle != 0 || this.minSlideAngle != 0){
				var rigidbody = this.GetComponent<Rigidbody>();
				RaycastHit slopeHit;
				bool slopeTest = rigidbody.SweepTest(-Vector3.up,out slopeHit,0.5f);
				if(slopeTest){
					return this.CheckSlope(current,slopeHit);
				}
			}
			return false;
		}
		private bool CheckStepUp(Vector3 current){
			if(this.maxStepHeight != 0 && current.y == 0){
				var rigidbody = this.GetComponent<Rigidbody>();
				RaycastHit stepHit;
				Vector3 move = this.NullBlocked(current) * this.GetTimeOffset();
				Vector3 direction = move.normalized;
				Vector3 position = rigidbody.position;
				float distance = Vector3.Distance(position,position+move) + this.hoverDistance*2;
				this.SetPosition(rigidbody.position + (Vector3.up * this.maxStepHeight));
				bool stepTest = rigidbody.SweepTest(direction,out stepHit,distance);
				if(!stepTest){
					this.SetPosition(rigidbody.position + move);
					rigidbody.SweepTest(-Vector3.up,out stepHit);
					this.SetPosition(rigidbody.position + -Vector3.up*(stepHit.distance-this.hoverDistance));
					this.blocked.down.Set(true);
					return true;
				}
				this.SetPosition(position);
			}
			return false;
		}
		private bool CheckStepDown(){
			RaycastHit stepHit;
			var rigidbody = this.GetComponent<Rigidbody>();
			if(rigidbody.SweepTest(-Vector3.up,out stepHit,this.maxStepHeight)){
				this.SetPosition(rigidbody.position + (-Vector3.up*(stepHit.distance-this.hoverDistance)));
				this.blocked.down.Set(true);
				return true;
			}
			return false;
		}
		//================================
		// Utility
		//================================
		private void SetPosition(Vector3 position){
			var rigidbody = this.GetComponent<Rigidbody>();
			rigidbody.position = position;
		}
		private void CheckActive(string state){
			var rigidbody = this.GetComponent<Rigidbody>();
			if(state == "Sleep"){rigidbody.Sleep();}
			if(state == "Wake"){rigidbody.WakeUp();}
		}
		private void CheckBlocked(){
			if(this.persistentBlockChecks){
				var rigidbody = this.GetComponent<Rigidbody>();
				RaycastHit hit;
				rigidbody.WakeUp();
				float distance = this.hoverDistance * 2 + 0.01f;
				this.blocked.forward.Set(rigidbody.SweepTest(this.transform.forward,out hit,distance));
				this.blocked.back.Set(rigidbody.SweepTest(-this.transform.forward,out hit,distance));
				this.blocked.up.Set(rigidbody.SweepTest(this.transform.up,out hit,distance));
				this.blocked.down.Set(rigidbody.SweepTest(-this.transform.up,out hit,distance));
				this.blocked.right.Set(rigidbody.SweepTest(this.transform.right,out hit,distance));
				this.blocked.left.Set(rigidbody.SweepTest(-this.transform.right,out hit,distance));
				rigidbody.Sleep();
			}
			if(this.blocked.forward){this.lastBlockedTime.forward.Set(Time.time);}
			if(this.blocked.back){this.lastBlockedTime.back.Set(Time.time);}
			if(this.blocked.up){this.lastBlockedTime.up.Set(Time.time);}
			if(this.blocked.down){this.lastBlockedTime.down.Set(Time.time);}
			if(this.blocked.right){this.lastBlockedTime.right.Set(Time.time);}
			if(this.blocked.left){this.lastBlockedTime.left.Set(Time.time);}
		}
		private Vector3 NullBlocked(Vector3 move){
			if(this.blocked.forward && move.x < 0){move.x = 0;}
			if(this.blocked.back && move.x > 0){move.x = 0;}
			if(this.blocked.up && move.y > 0){move.y = 0;}
			if(this.blocked.down && move.y < 0){move.y = 0;}
			if(this.blocked.right && move.z > 0){move.z = 0;}
			if(this.blocked.left && move.z < 0){move.z = 0;}
			return move;
		}
		private void ResetBlocked(bool clearTime=false){
			this.blocked.forward.Set(false);
			this.blocked.back.Set(false);
			this.blocked.up.Set(false);
			this.blocked.down.Set(false);
			this.blocked.right.Set(false);
			this.blocked.left.Set(false);
			if(clearTime){
				this.lastBlockedTime.forward.Set(0);
				this.lastBlockedTime.back.Set(0);
				this.lastBlockedTime.up.Set(0);
				this.lastBlockedTime.down.Set(0);
				this.lastBlockedTime.right.Set(0);
				this.lastBlockedTime.left.Set(0);
			}
		}
		//================================
		// Events
		//================================
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
		public void OnCollision(object data){
			if(!this.enabled){return;}
			var collision = (CollisionData)data;
			if(collision.isTrigger){this.hasTriggers = true;}
			this.frameCollisions[collision.hitObject] = collision;
		}
		public void OnCollisionStart(object data){
			if(!this.enabled){return;}
			var collision = (CollisionData)data;
			this.collisions[collision.hitObject] = collision;
		}
		public void OnCollisionEnd(object data){
			if(!this.enabled){return;}
			var collision = (CollisionData)data;
			this.collisions.Remove(collision.hitObject);
			this.hasTriggers = this.collisions.Values.ToList().Exists(x=>x.isTrigger);
		}
	}
}