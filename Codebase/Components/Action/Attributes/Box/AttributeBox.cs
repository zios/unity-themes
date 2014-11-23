using Zios;
using System.Collections;
using UnityEngine;
[AddComponentMenu("")]
public class AttributeBox<Type> : AttributeBox 
where Type : Zios.Attribute,new(){
	public string alias = "Attribute";
	public Type value = new Type();
	public override void OnApplicationQuit(){this.Awake();}
	public override void Reset(){this.Awake();}
	public override void Awake(){
		this.alias = this.alias.SetDefault("Attribute");
		this.value.Setup("",this);
	}
}
public class AttributeBox : MonoBehaviour{
	public virtual void OnApplicationQuit(){}
	public virtual void Reset(){}
	public virtual void Awake(){}
}