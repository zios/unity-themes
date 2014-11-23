using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Raycast")]
public class RayCast : ActionPart{
	public AttributeFloat distance = 1;
	public Color rayColor = Color.blue;
	public AttributeVector3 direction = -Vector3.up;
	public AttributeVector3 offset = Vector3.zero;
	public AttributeGameObject source = new AttributeGameObject();
	public LayerMask layers = -1;
	public AttributeBool relative = false;
	public override void Awake(){
		base.Awake();
		this.DefaultPriority(5);
		this.distance.Setup("Distance",this);
		this.direction.Setup("Direction",this);
		this.offset.Setup("Offset",this);
		this.relative.Setup("Relative",this);
		this.source.Setup("Source",this);
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
		bool state = Physics.Raycast(position,direction,distance,this.layers.value);
		this.Toggle(state);
	}
	public void OnDrawGizmosSelected(){
		GameObject source = this.source.Get();
		if(!source.IsNull()){
			Gizmos.color = this.rayColor;
			Vector3 direction = this.AdjustVector(this.direction);
			Vector3 start = source.transform.position + this.AdjustVector(this.offset);
			Vector3 end = start + (direction * this.distance);
			Gizmos.DrawLine(start,end);
		}
	}
}
