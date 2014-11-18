using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Modify (Bool)")]
public class AttributeModifyBool : ActionPart{
	public AttributeBool target = false;
	public AttributeBool value = false;
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.target.mode = AttributeMode.Linked;
		this.value.Setup("Value",this);
	}
	public override void Use(){
		this.target.Set(this.value.Get());
		base.Use();
	}
}
