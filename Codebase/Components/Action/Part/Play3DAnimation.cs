using UnityEngine;
using System;
using Zios;
[AddComponentMenu("Zios/Component/Action/Part/Play Animation (3D)")]
public class Play3DAnimation : ActionPart{
	public AttributeString animationName;
	public AttributeFloat speed = 1;
	public AttributeFloat weight = 1;
	public Target target;
	public override void OnValidate(){
		base.OnValidate();
		this.animationName.Setup("AnimationName",this);
		this.speed.Setup("Speed",this);
		this.weight.Setup("Weight",this);
		this.target.Setup("Target",this);
	}
	public override void Use(){
		base.Use();
		this.target.Call("SetAnimationSpeed",this.animationName,this.speed.Get());
		this.target.Call("SetAnimationWeight",this.animationName,this.weight.Get());
		this.target.Call("PlayAnimation",this.animationName);
	}
	public override void End(){
		base.End();
		this.target.Call("StopAnimation",this.animationName);
	}
}
