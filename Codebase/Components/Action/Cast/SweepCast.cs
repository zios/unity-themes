using UnityEngine;
namespace Zios.Actions.CastComponents{
	using Attributes;
	using Motion;
	[AddComponentMenu("Zios/Component/Action/Cast/Sweepcast")]
	public class SweepCast : StateMonoBehaviour{
		public AttributeGameObject source = new AttributeGameObject();
		public AttributeVector3 goal = -Vector3.up;
		public AttributeFloat distance = 1;
		[Advanced] public Color debugColor = Color.blue;
		[Internal] public AttributeVector3 hitPoint = Vector3.zero;
		[Internal] public AttributeVector3 hitNormal = Vector3.zero;
		[Internal] public AttributeFloat hitDistance = 0;
		[Internal] public AttributeGameObject hit = new AttributeGameObject();
		[Internal] public RaycastHit castHit = new RaycastHit();
		public override void Awake(){
			base.Awake();
			this.source.Setup("Source",this);
			this.goal.Setup("Goal",this);
			this.distance.Setup("Distance",this);
			this.hit.Setup("Hit",this);
			this.hitPoint.Setup("Hit Point",this);
			this.hitNormal.Setup("Hit Normal",this);
			this.hitDistance.Setup("Hit Distance",this);
			this.AddDependent(this.source,false,typeof(Rigidbody),typeof(ColliderController));
		}
		public override void Use(){
			bool state = false;
			if(!this.source.Get().IsNull() && !this.source.Get().GetComponent<Rigidbody>().IsNull()){
				var rigidBody = this.source.Get().GetComponent<Rigidbody>();
				float distance = this.distance == -1 ? Mathf.Infinity : this.distance.Get();
				state = rigidBody.SweepTest(this.goal,out this.castHit,distance);
				if(state){
					this.hit.Set(this.castHit.collider.gameObject);
					this.hitPoint.Set(this.castHit.point);
					this.hitNormal.Set(this.castHit.normal);
					this.hitDistance.Set(this.castHit.distance);
				}
			}
			this.Toggle(state);
		}
		public void OnDrawGizmosSelected(){
			if(!Attribute.ready){return;}
			if(!this.source.Get().IsNull()){
				Gizmos.color = this.debugColor;
				Vector3 start = this.source.Get().transform.position;
				Vector3 end = start + (this.goal * this.distance);
				Gizmos.DrawLine(start,end);
			}
		}
	}
}