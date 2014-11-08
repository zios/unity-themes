using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Modify (Int)")]
public class AttributeModifyInt : ActionPart{
	public AttributeInt target = 0;
	public AttributeInt value = 0;
	public override void Start(){
		base.Start();
		this.target.Setup("Target",this);
		this.target.mode = AttributeMode.Linked;
		this.value.Setup("Value",this);
	}
	public override void Use(){
		this.target.Set(this.value.Get());
		base.Use();
	}
}
