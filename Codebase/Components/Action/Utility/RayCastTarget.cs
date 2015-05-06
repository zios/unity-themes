using Zios;
using System;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Raycast (Target)")]
    public class RayCastTarget : ActionLink{
	    public AttributeGameObject source = new AttributeGameObject();
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
		    Vector3 sourcePosition = this.source.Get().transform.position;
		    bool state = Physics.Raycast(sourcePosition,this.direction,out this.castHit,distance,this.layers.value);
			if(state){
				this.hit.Set(this.castHit.collider.gameObject);
				this.hitPoint.Set(this.castHit.point);
				this.hitNormal.Set(this.castHit.normal);
				this.hitDistance.Set(this.castHit.distance);
			}
		    this.Toggle(state);
	    }
	    public void OnDrawGizmosSelected(){
			Gizmos.color = this.debugColor;
		    GameObject source = this.source.Get();
		    if(!source.IsNull()){
			    Vector3 start = source.transform.position;
			    Vector3 end = start + (direction * this.distance);
			    Gizmos.DrawLine(start,end);
		    }
	    }
    }
}
