using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Modify (Float)")]
public class AttributeModifyFloat : ActionPart{
	public AttributeFloat target = 0;
	public AttributeFloat value = 0;
	public override void OnValidate(){
		base.OnValidate();
		this.target.Setup("Target",this);
		this.target.mode = AttributeMode.Linked;
		this.value.Setup("Value",this);
	}
	public override void Use(){
		this.target.Set(this.value.Get());
		base.Use();
	}
}
