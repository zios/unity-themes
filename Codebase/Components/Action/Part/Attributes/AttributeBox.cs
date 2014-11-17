using Zios;
using System.Collections;
using UnityEngine;
using UnityEditor;
[AddComponentMenu("")]
public class AttributeBox<Type> : AttributeBox 
where Type : Zios.Attribute,new(){
	public static float nextUpdate;
	public string alias = "Attribute";
	public Type value = default(Type);
	public virtual void Reset(){this.Awake();}
	public virtual void OnApplicationQuit(){this.Awake();}
	public override void Awake(){
		this.alias = this.alias.SetDefault("Attribute");
		this.value.Setup("",this);
	}
}
public class AttributeBox : MonoBehaviour{
	public virtual void Awake(){}
}