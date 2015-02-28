using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Event/Collision Listen")]
public class CollisionListen : ActionLink{
	public CollisionEvent trigger;
	[EnumMask] public CollisionSource sourceCause = (CollisionSource)(-1);
	//[EnumMask] public CollisionDirection direction = (CollisionDirection)(-1);
	public LayerMask layer = (LayerMask)(-1);
	public AttributeGameObject target = new AttributeGameObject();
	//public AttributeBool forceRequired = true;
	[HideInInspector] public AttributeGameObject lastCollision = new AttributeGameObject();
	public override void Awake(){
		base.Awake();
		this.AddDependent<ColliderController>(this.target);
		this.lastCollision.Setup("Last Collision",this);
		this.target.Setup("Target",this);
		this.target.DefaultSearch("[Owner]");
		if(!this.target.Get().IsNull()){
			Events.AddScope(this.trigger.ToString(),(MethodObject)this.Collision,this.target.Get());
		}
	}
	public override void Use(){}
	public void Collision(object data){
		CollisionData collision = (CollisionData)data;
		CollisionSource sourceCause = collision.isSource ? CollisionSource.Self : CollisionSource.Target;
		this.lastCollision.Set(collision.gameObject);
		bool layerMatch = this.layer.Contains(collision.gameObject.layer);
		bool sourceMatch = this.sourceCause.Contains(sourceCause);
		if(sourceMatch && layerMatch /*&& directionMatch && this.forceRequired.Get()*/){
			base.Use();
		}
	}
}
public enum CollisionEvent{CollisionStart,CollisionEnd,Collision}
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