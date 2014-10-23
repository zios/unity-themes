using Zios;
using System;
using UnityEngine;
public enum OffsetType{Relative,Absolute}
[AddComponentMenu("Zios/Component/Action/Part/Follow Target")]
public class FollowTarget : ActionPart{
	public Target source = new Target();
	public Target target = new Target();
	public LerpVector3 position = new LerpVector3();
	public OffsetType offsetType;
	public EventVector3 offset;
	public EventVector3 orbit;
	public override void OnValidate(){
		this.DefaultPriority(5);
		base.OnValidate();
		this.source.Update(this);
		this.target.Update(this);
	}
	public void Start(){
		this.source.Setup(this,"Source");
		this.target.Setup(this);
		this.position.Setup(this,"Follow");
		this.offset.Setup(this,"FollowOffset");
		this.orbit.Setup(this,"FollowOrbit");
	}
	public Vector3 AdjustVector(Vector3 value){
		Vector3 adjusted = value;
		if(this.offsetType == OffsetType.Relative){
			Transform target = this.target.direct.transform;
			adjusted = target.right * value.x;
			adjusted += target.up * value.y;
			adjusted += target.forward * value.z;
		}
		return adjusted;
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform target = this.target.Get().transform;
		Vector3 offset = this.AdjustVector(this.offset);
		Vector3 orbit = this.orbit.Scale(new Vector3(1,-1,1));
		Vector3 end = (orbit.ToRotation() * offset) + target.position;
		source.position = this.position.Step(source.position,end);
		base.Use();
	}
}

