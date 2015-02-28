using Zios;
using System;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Linecast")]
    public class LineCast : ActionLink{
	    public Color rayColor = Color.blue;
	    public AttributeVector3 offset = Vector3.zero;
	    public AttributeGameObject source = new AttributeGameObject();
	    public AttributeGameObject target = new AttributeGameObject();
	    public LayerMask layers = -1;
	    public AttributeBool relative = false;
	    [HideInInspector] public RaycastHit rayHit = new RaycastHit();
	    [HideInInspector] public AttributeVector3 hitPoint = Vector3.zero;
	    [HideInInspector] public AttributeVector3 hitNormal = Vector3.zero;
	    [HideInInspector] public AttributeFloat hitDistance = 0;
	    public override void Awake(){
		    base.Awake();
		    this.offset.Setup("Offset",this);
		    this.relative.Setup("Relative",this);
		    this.source.Setup("Source",this);
		    this.target.Setup("Target",this);
		    this.target.DefaultSearch("[Owner]");
		    this.hitPoint.Setup("Hit Point",this);
		    this.hitNormal.Setup("Hit Normal",this);
		    this.hitDistance.Setup("Hit Distance",this);
	    }
	    public Vector3 GetPosition(){
		    GameObject source = this.source.Get();
		    if(!source.IsNull()){
			    return source.transform.position;
		    }
		    return Vector3.zero;
	    }
	    public Vector3 GetTargetPosition(){
		    GameObject target = this.target.Get ();
		    if(!target.IsNull()){
			    return target.transform.position;
		    }
		    return Vector3.zero;
	    }
	    public Vector3 AdjustVector(Vector3 value){
		    Vector3 adjusted = value;
		    if(this.relative){
			    Transform source = this.source.Get().transform;
			    adjusted = source.right * value.x;
			    adjusted += source.up * value.y;
			    adjusted += source.forward * value.z;
		    }
		    return adjusted;
	    }
	    public override void Use(){
		    Vector3 position = this.GetPosition() + this.AdjustVector(this.offset);
		    Vector3 targetPosition = this.GetTargetPosition();
		    bool state = Physics.Linecast(position,targetPosition,out rayHit,this.layers.value);
		    this.Toggle(state);
		    this.hitPoint.Set(rayHit.point);
		    this.hitNormal.Set(rayHit.normal);
		    this.hitDistance.Set(rayHit.distance);
	    }
	    public void OnDrawGizmosSelected(){
		    GameObject source = this.source.Get();
		    GameObject target = this.target.Get();
		    if(!source.IsNull() && !target.IsNull()){
			    Gizmos.color = this.rayColor;
			    Vector3 start = source.transform.position + this.AdjustVector(this.offset);
			    Gizmos.DrawLine(start,target.transform.position);
		    }
	    }
    }
}