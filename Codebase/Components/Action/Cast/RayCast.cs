using UnityEngine;
namespace Zios.Actions.CastComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Cast/Raycast")]
	public class RayCast : StateMonoBehaviour{
		public AttributeVector3 source = new AttributeVector3();
		public AttributeVector3 direction = -Vector3.up;
		public AttributeFloat distance = 1;
		public LayerMask layers = -1;
		[Advanced] public Color debugColor = Color.blue;
		[Internal] public AttributeVector3 hitPoint = Vector3.zero;
		[Internal] public AttributeVector3 hitNormal = Vector3.zero;
		[Internal] public AttributeFloat hitDistance = 0;
		[Internal] public AttributeGameObject hit = new AttributeGameObject();
		[Internal] public RaycastHit castHit = new RaycastHit();
		public override void Awake(){
			base.Awake();
			this.source.Setup("Source",this);
			this.direction.Setup("Direction",this);
			this.distance.Setup("Distance",this);
			this.hit.Setup("Hit",this);
			this.hitPoint.Setup("Hit Point",this);
			this.hitNormal.Setup("Hit Normal",this);
			this.hitDistance.Setup("Hit Distance",this);
		}
		public override void Use(){
			float distance = this.distance == -1 ? Mathf.Infinity : this.distance.Get();
			bool state = Physics.Raycast(this.source,this.direction,out this.castHit,distance,this.layers.value);
			if(state){
				this.hit.Set(this.castHit.collider.gameObject);
				this.hitPoint.Set(this.castHit.point);
				this.hitNormal.Set(this.castHit.normal);
				this.hitDistance.Set(this.castHit.distance);
			}
			this.Toggle(state);
		}
		public void OnDrawGizmosSelected(){
			if(!Attribute.ready){return;}
			Gizmos.color = this.debugColor;
			Vector3 start = this.source;
			Vector3 end = start + (direction * this.distance);
			Gizmos.DrawLine(start,end);
		}
	}
}