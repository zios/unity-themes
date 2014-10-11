using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Ray State")]
public class RayState : ActionPart{
	public float distance = 1;
	public Color rayColor = Color.blue;
	public Vector3 direction = -Vector3.up;
	public Vector3 offset;
	public Target source = new Target();
	public LayerMask layers = -1;
	public bool relative;
	public override void OnValidate(){
		this.DefaultPriority(5);
		base.OnValidate();
		this.source.AddSpecial("[Owner]",this.action.owner);
		this.source.AddSpecial("[Action]",this.action.gameObject);
		this.source.DefaultSearch("[Owner]");
	}
	public Vector3 GetPosition(){
		if(this.source.Get() != null){
			return this.source.direct.transform.position;
		}
		return Vector3.zero;
	}
	public Vector3 GetDirection(){
		Vector3 direction = this.direction;
		if(this.relative){
			Transform target = this.source.direct.transform;
			direction = target.right * this.direction.x;
			direction += target.up * this.direction.y;
			direction += target.forward * this.direction.z;
		}
		return direction;
	}
	public override void Use(){
		float distance = this.distance == -1 ? Mathf.Infinity : this.distance;
		Vector3 direction = this.GetDirection();
		Vector3 position = this.GetPosition() + this.offset;
		bool state = Physics.Raycast(position,direction,distance,this.layers.value);
		this.Toggle(state);
	}
	public void OnDrawGizmosSelected(){
		if(this.source.direct != null){
			Gizmos.color = this.rayColor;
			Vector3 direction = this.GetDirection();
			Vector3 start = this.source.direct.transform.position + this.offset;
			Vector3 end = start + (direction * this.distance);
			Gizmos.DrawLine(start,end);
		}
	}
}