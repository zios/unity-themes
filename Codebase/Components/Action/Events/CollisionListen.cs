using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Event/Collision Listen")]
public enum CollisionDirection : int{
	Above     = 0x001,
	Below     = 0x002,
	Front     = 0x004,
	Behind    = 0x008,
	Left      = 0x010,
	Right     = 0x020,
}
public enum CollisionSource : int{
	Self      = 0x001,
	Target    = 0x002,
}
public class CollisionListen : ActionPart{
	[EnumMask] public CollisionSource sourceCause = (CollisionSource)(-1);
	//[EnumMask] public CollisionDirection direction = (CollisionDirection)(-1);
	public LayerMask layer = (LayerMask)(-1);
	public Target target = new Target();
	//public AttributeBool forceRequired = true;
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.target.DefaultSearch("[Owner]");
		if(!this.target.direct.IsNull()){
			Events.AddScope("Collide",(MethodObject)this.OnCollide,this.target.direct);
		}
	}
	public override void Use(){}
	public void OnCollide(object data){
		CollisionData collision = (CollisionData)data;
		CollisionSource sourceCause = collision.isSource ? CollisionSource.Self : CollisionSource.Target;
		bool layerMatch = this.layer.Contains(collision.gameObject.layer);
		bool sourceMatch = this.sourceCause.Contains(sourceCause);
		if(sourceMatch && layerMatch /*&& directionMatch && this.forceRequired.Get()*/){
			base.Use();
		}
	}
}
