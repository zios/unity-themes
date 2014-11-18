using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Modify (String)")]
public class AttributeModifyString : ActionPart{
	public AttributeString target = "";
	public AttributeString value = "";
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
