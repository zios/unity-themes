using UnityEngine;
using System;
using Zios;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Animation Trigger")]
public class AnimationTrigger : ActionPart{
	public string animationName;
	public int animationPriority = -1;
	public float holdDuration = 0;
	public float speed = 1;
	public bool speedBasedOnIntensity;
	public void OnValidate(){this.DefaultPriority(15);}
	public override void Use(){
		base.Use();
		if(this.speedBasedOnIntensity){this.speed = this.action.intensity;}
		this.action.owner.Call("SetAnimation",this.animationName,true,this.animationPriority);
		this.action.owner.Call("SetAnimationSpeed",this.animationName,this.speed);
	}
	public override void End(){
		base.End();
		this.action.owner.Call("HoldAnimation",this.holdDuration);
		this.action.owner.Call("SetAnimation",this.animationName,false,this.animationPriority);
	}
} 