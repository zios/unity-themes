using UnityEngine;
namespace Zios.Motion{
	using Attributes;
	using Events;
	[AddComponentMenu("Zios/Component/Motion/Force")]
	public class Force : ManagedMonoBehaviour{
		public AttributeVector3 velocity = Vector3.zero;
		public AttributeVector3 terminalVelocity = new Vector3(20,20,20);
		public AttributeVector3 resistence = new Vector3(8,0,8);
		public AttributeFloat minimumImpactVelocity = 1;
		public AttributeBool disabled = false;
		public override void Awake(){
			base.Awake();
			this.velocity.Setup("Velocity",this);
			this.terminalVelocity.Setup("Terminal Velocity",this);
			this.resistence.Setup("Resistence",this);
			this.minimumImpactVelocity.Setup("Minimum Impact Velocity",this);
			this.disabled.Setup("Disabled",this);
			Event.Register("On Impact",this.gameObject);
			Event.Register("Add Move",this.gameObject);
			Event.Add("On Collision",(MethodObject)this.OnCollide,this.gameObject);
			Event.Add("Add Force",(MethodVector3)this.AddForce,this.gameObject);
			Event.Add("Add Force Raw",(MethodVector3)this.AddForceRaw,this.gameObject);
			this.AddDependent<ColliderController>(this.gameObject);
		}
		public override void Step(){
			if(!this.disabled && this.velocity != Vector3.zero){
				Vector3 resistence = Vector3.Scale(this.velocity.Get().Sign(),this.resistence);
				this.velocity.Set(this.velocity - resistence * this.GetTimeOffset());
				this.velocity.Set(this.velocity.Get().Clamp(this.terminalVelocity.Get()*-1,this.terminalVelocity));
				this.gameObject.CallEvent("Add Move",new Vector3(this.velocity.x,0,0));
				this.gameObject.CallEvent("Add Move",new Vector3(0,this.velocity.y,0));
				this.gameObject.CallEvent("Add Move",new Vector3(0,0,this.velocity.z));
			}
		}
		public void AddForce(Vector3 force){
			force *= this.GetTimeOffset();
			this.velocity.Set(this.velocity + force);
		}
		public void AddForceRaw(Vector3 force){
			this.velocity.Set(this.velocity + force);
		}
		public void OnCollide(object collision){
			CollisionData data = (CollisionData)collision;
			if(data.isSource){
				Vector3 original = this.velocity.Get();
				if(data.sourceController.blocked.forward && this.velocity.z < 0){this.velocity.z.Set(0);}
				if(data.sourceController.blocked.back && this.velocity.z > 0){this.velocity.z.Set(0);}
				if(data.sourceController.blocked.up && this.velocity.y > 0){this.velocity.y.Set(0);}
				if(data.sourceController.blocked.down && this.velocity.y < 0){this.velocity.y.Set(0);}
				if(data.sourceController.blocked.right && this.velocity.x > 0){this.velocity.x.Set(0);}
				if(data.sourceController.blocked.left && this.velocity.x < 0){this.velocity.x.Set(0);}
				if(original != this.velocity.Get()){
					Vector3 impact = (this.velocity - original);
					float impactStrength = impact.magnitude;
					if(impactStrength > this.minimumImpactVelocity){
						this.gameObject.CallEvent("On Impact",impact);
					}
				}
			}
		}
	}
}