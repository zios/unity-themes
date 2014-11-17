using UnityEngine;
using System;
using Zios;
[AddComponentMenu("Zios/Component/Action/Part/Play Animation (3D)")]
public class Play3DAnimation : ActionPart{
	public AttributeString animationName = "";
	public AttributeFloat speed = 1;
	public AttributeFloat weight = 1;
	public Target target;
	public override void Awake(){
		base.Awake();
		this.animationName.Setup("Animation Name",this);
		this.speed.Setup("Speed",this);
		this.weight.Setup("Weight",this);
		this.target.Setup("Target",this);
	}
	public override void Use(){
		base.Use();
		string name = this.animationName.Get();
		this.target.Call("Set Animation Speed",name,this.speed.Get());
		this.target.Call("Set Animation Weight",name,this.weight.Get());
		this.target.Call("Play Animation",name);
	}
	public override void End(){
		base.End();
		this.target.Call("Stop Animation",this.animationName.Get());
	}
}
