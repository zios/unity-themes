using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Raycast")]
public class RayCast : ActionLink{
	public AttributeFloat distance = 1;
	public Color debugColor = Color.blue;
	public AttributeVector3 direction = -Vector3.up;
	public AttributeVector3 offset = Vector3.zero;
	public AttributeGameObject source = new AttributeGameObject();
	[HideInInspector] public RaycastHit rayHit = new RaycastHit();
	[HideInInspector] public AttributeVector3 hitPoint = Vector3.zero;
	[HideInInspector] public AttributeVector3 hitNormal = Vector3.zero;
	[HideInInspector] public AttributeFloat hitDistance = 0;
	public LayerMask layers = -1;
	public AttributeBool relative = false;
	public override void Awake(){
		base.Awake();
		this.distance.Setup("Distance",this);
		this.direction.Setup("Direction",this);
		this.offset.Setup("Offset",this);
		this.relative.Setup("Relative",this);
		this.source.Setup("Source",this);
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
		float distance = this.distance == -1 ? Mathf.Infinity : this.distance.Get();
		Vector3 direction = this.AdjustVector(this.direction);
		Vector3 position = this.GetPosition() + this.AdjustVector(this.offset);
		bool state = Physics.Raycast(position,direction,out rayHit,distance,this.layers.value);
		this.hitPoint.Set(rayHit.point);
		this.hitNormal.Set(rayHit.normal);
		this.hitDistance.Set(rayHit.distance);
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
