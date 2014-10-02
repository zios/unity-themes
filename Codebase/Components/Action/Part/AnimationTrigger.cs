using UnityEngine;
using System;
using Zios;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Animation Trigger")]
public class AnimationTrigger : ActionPart{
	public string animationName;
	public float speed = 1;
	public float weight = 1;
	public bool speedBasedOnIntensity;
	public bool blendBasedOnIntensity;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		base.Use();
		if(this.speedBasedOnIntensity){this.speed = this.action.intensity;}
		if(this.blendBasedOnIntensity){this.weight = this.action.intensity;}
		if(this.speed != 1 || this.speedBasedOnIntensity){this.action.owner.Call("SetAnimationSpeed",this.animationName,this.speed);}
		if(this.weight != 1 || this.blendBasedOnIntensity){this.action.owner.Call("SetAnimationWeight",this.animationName,this.weight);}
		this.action.owner.Call("SetAnimation",this.animationName,true);
	}
	public override void End(){
		base.End();
		this.action.owner.Call("SetAnimation",this.animationName,false);
	}
}