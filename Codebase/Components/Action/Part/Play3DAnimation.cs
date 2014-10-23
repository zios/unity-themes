using UnityEngine;
using System;
using Zios;
[AddComponentMenu("Zios/Component/Action/Part/Play Animation (3D)")]
public class Play3DAnimation : ActionPart{
	public string animationName;
	public float speed = 1;
	public float weight = 1;
	public bool speedBasedOnIntensity;
	public bool blendBasedOnIntensity;
	public Target target;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
	}
	public override void Use(){
		base.Use();
		if(this.speedBasedOnIntensity){this.speed = this.action.intensity;}
		if(this.blendBasedOnIntensity){this.weight = this.action.intensity;}
		if(this.speed != 1 || this.speedBasedOnIntensity){this.target.Call("SetAnimationSpeed",this.animationName,this.speed);}
		if(this.weight != 1 || this.blendBasedOnIntensity){this.target.Call("SetAnimationWeight",this.animationName,this.weight);}
		this.target.Call("SetAnimation",this.animationName,true);
	}
	public override void End(){
		base.End();
		this.target.Call("SetAnimation",this.animationName,false);
	}
}
