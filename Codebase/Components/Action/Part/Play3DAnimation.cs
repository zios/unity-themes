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
		this.animationName.Setup("Animation Name",this);
		this.speed.Setup("Speed",this);
		this.weight.Setup("Weight",this);
		this.target.Setup("Target",this);
	}
	public override void Use(){
		base.Use();
		string name = this.animationName.Get();
		this.target.Call("SetAnimationSpeed",name,this.speed.Get());
		this.target.Call("SetAnimationWeight",name,this.weight.Get());
		this.target.Call("PlayAnimation",name);
	}
	public override void End(){
		base.End();
		this.target.Call("StopAnimation",this.animationName.Get());
	}
}
