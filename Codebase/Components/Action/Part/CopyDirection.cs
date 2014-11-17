using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute/Copy Direction")]
public class CopyDirection : ActionPart{
	public Direction direction;
	public Target source = new Target();
	public AttributeVector3 target = Vector3.zero;
	public override void Awake(){
		base.Awake();
		this.DefaultPriority(5);
		this.source.Setup("Source",this);
		this.target.Setup("Target",this);
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Vector3 direction = Vector3.zero;
		if(this.direction == Direction.Up){direction = source.rotation * Vector3.up;}
		if(this.direction == Direction.Down){direction = source.rotation * Vector3.down;}
		if(this.direction == Direction.Left){direction = source.rotation * Vector3.left;}
		if(this.direction == Direction.Right){direction = source.rotation * Vector3.right;}
		if(this.direction == Direction.Forward){direction = source.rotation * Vector3.forward;}
		if(this.direction == Direction.Back){direction = source.rotation * Vector3.back;}
		this.target.Set(direction);
		base.Use();
	}
}

