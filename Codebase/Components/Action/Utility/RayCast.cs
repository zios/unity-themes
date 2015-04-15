using Zios;
using System;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Raycast")]
    public class RayCast : ActionLink{
	    public AttributeGameObject source = new AttributeGameObject();
	    public AttributeVector3 offset = Vector3.zero;
	    public AttributeVector3 direction = -Vector3.up;
	    public AttributeFloat distance = 1;
	    public Color debugColor = Color.blue;
	    [HideInInspector] public RaycastHit castHit = new RaycastHit();
	    [HideInInspector] public AttributeVector3 hitPoint = Vector3.zero;
	    [HideInInspector] public AttributeVector3 hitNormal = Vector3.zero;
	    [HideInInspector] public AttributeFloat hitDistance = 0;
	    public LayerMask layers = -1;
	    public AttributeBool relative = false;
	    public override void Awake(){
		    base.Awake();
		    this.source.Setup("Source",this);
		    this.direction.Setup("Direction",this);
		    this.offset.Setup("Offset",this);
		    this.distance.Setup("Distance",this);
		    this.relative.Setup("Relative",this);
		    this.hitPoint.Setup("Hit Point",this);
		    this.hitNormal.Setup("Hit Normal",this);
		    this.hitDistance.Setup("Hit Distance",this);
	    }
	    public Vector3 AdjustVector(Vector3 value){
		    if(this.relative){
			    return this.source.Get().transform.Localize(value);
		    }
		    return value;
	    }
	    public override void Use(){
		    float distance = this.distance == -1 ? Mathf.Infinity : this.distance.Get();
		    Vector3 direction = this.AdjustVector(this.direction);
		    Vector3 sourcePosition = this.source.Get().transform.position + this.AdjustVector(this.offset);
		    bool state = Physics.Raycast(sourcePosition,direction,out this.castHit,distance,this.layers.value);
		    this.hitPoint.Set(this.castHit.point);
		    this.hitNormal.Set(this.castHit.normal);
		    this.hitDistance.Set(this.castHit.distance);
		    this.Toggle(state);
	    }
	    public void OnDrawGizmosSelected(){
		    GameObject source = this.source.Get();
		    if(!source.IsNull()){
			    Gizmos.color = this.debugColor;
			    Vector3 direction = this.AdjustVector(this.direction);
			    Vector3 start = source.transform.position + this.AdjustVector(this.offset);
			    Vector3 end = start + (direction * this.distance);
			    Gizmos.DrawLine(start,end);
		    }
	    }
    }
}