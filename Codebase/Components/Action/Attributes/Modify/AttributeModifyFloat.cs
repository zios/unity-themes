using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Attribute/Modify/Modify Float")]
public class AttributeModifyFloat : ActionLink{
	public AttributeFloat target = 0;
	public AttributeFloat value = 0;
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.target.info.mode = AttributeMode.Linked;
		this.value.Setup("Value",this);
	}
	public override void Use(){
		this.target.Set(this.value.Get());
		base.Use();
	}
}
