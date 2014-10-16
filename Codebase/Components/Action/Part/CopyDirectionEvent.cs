using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Copy Direction (Event)")]
public class CopyDirectionEvent : ActionPart{
	public Direction direction;
	public Target source = new Target();
	public EventSetTarget target = new EventSetTarget();
	public override void OnValidate(){
		this.DefaultPriority(5);
		base.OnValidate();
		this.source.Update(this);
		this.target.Update(this);
	}
	public void Start(){
		this.source.Setup(this,"Source");
		this.target.Setup(this);
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

