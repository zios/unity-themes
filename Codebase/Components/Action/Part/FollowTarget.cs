using Zios;
using System;
using UnityEngine;
public enum OffsetType{Relative,Absolute}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Follow Target")]
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
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform target = this.target.Get().transform;
		Vector3 offset = this.offset;
		Vector3 orbit = this.orbit.Scale(new Vector3(1,-1,1));
		if(this.offsetType == OffsetType.Relative){
			Vector3 current = this.offset;
			offset = target.forward * current.z;
			offset += target.up * current.y;
			offset += target.right * current.x;
		}
		Vector3 end = (orbit.ToRotation() * offset) + target.position;
		source.position = this.position.Step(source.position,end);
		base.Use();
	}
}

