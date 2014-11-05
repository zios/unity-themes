using Zios;
using System;
using UnityEngine;
[AddComponentMenu("")]
public class AttributeBox<Type> : MonoBehaviour 
where Type : Zios.Attribute{
	public string alias = "Attribute";
	public Type value = default(Type);
	public void OnValidate(){
		this.alias = this.alias.SetDefault("Attribute");
		this.value.Setup("",this);
	}
}