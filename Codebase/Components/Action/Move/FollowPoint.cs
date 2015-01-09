using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Move/Follow Point")]
public class FollowPoint : ActionLink{
	public AttributeGameObject target = new AttributeGameObject();
	public AttributeVector3 goal = Vector3.zero;
	public LerpVector3 position = new LerpVector3();
	public AttributeVector3 orbit = Vector3.zero;
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.goal.Setup("Goal",this);
		this.position.Setup("Follow",this);
		this.orbit.Setup("Follow Orbit",this);
	}
	public override void Use(){
		Transform target = this.target.Get().transform;
		Vector3 orbit = this.orbit.Scale(new Vector3(1,-1,1));
		Vector3 end = orbit.ToRotation()*Vector3.zero + this.goal.Get();
		target.position = this.position.Step(target.position,end);
		base.Use();
	}
}

