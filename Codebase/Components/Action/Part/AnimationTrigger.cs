using UnityEngine;
using System;
using Zios;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Animation Trigger")]
public class AnimationTrigger : ActionPart{
	public string animationName;
	[HideInInspector] public int animationPriority = -1;
	[HideInInspector] public float holdDuration = -1;
	public float speed = 1;
	public float weight = 1;
	public bool speedBasedOnIntensity;
	public bool blendBasedOnIntensity;
	public void OnValidate(){this.DefaultPriority(15);}
	public override void Use(){
		base.Use();
		if(this.speedBasedOnIntensity){this.speed = this.action.intensity;}
		if(this.blendBasedOnIntensity){this.weight = this.action.intensity;}
		this.action.owner.Call("SetAnimation",this.animationName,true);
		this.action.owner.Call("SetAnimationSpeed",this.animationName,this.speed);
		this.action.owner.Call("SetAnimationWeight",this.animationName,this.weight);

	}
	public override void End(){
		base.End();
		this.action.owner.Call("SetAnimation",this.animationName,false);
	}
}