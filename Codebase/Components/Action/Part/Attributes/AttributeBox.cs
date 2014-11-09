using Zios;
using System;
using UnityEngine;
[AddComponentMenu("")][ExecuteInEditMode]
public class AttributeBox<Type> : MonoBehaviour 
where Type : Zios.Attribute{
	public static float nextUpdate;
	public string alias = "Attribute";
	public Type value = default(Type);
	public void OnValidate(){this.Start();}
	public void Start(){
		this.alias = this.alias.SetDefault("Attribute");
		this.value.Setup("",this);
	}
}