using Zios;
using System.Collections;
using UnityEngine;
[AddComponentMenu("")]
public class AttributeBox<AttributeType> : DataMonoBehaviour
where AttributeType : Zios.Attribute,new(){
	public AttributeType value = new AttributeType();
	public bool remember = false;
	public override void OnApplicationQuit(){
		if(this.remember){this.Store();}
		this.Awake();
	}
	public virtual void Load(){}
	public virtual void Store(){}
	public override void Reset(){this.Awake();}
	public override void Awake(){
		this.alias = this.alias.SetDefault("Attribute");
		this.value.Setup("",this);
		if(this.remember){this.Load();}
	}
}