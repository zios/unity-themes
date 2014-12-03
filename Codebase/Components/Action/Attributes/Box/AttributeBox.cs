using Zios;
using System.Collections;
using UnityEngine;
[AddComponentMenu("")]
public class AttributeBox<AttributeType> : DataMonoBehaviour
where AttributeType : Zios.Attribute,new(){
	public AttributeType value = new AttributeType();
	public override void OnApplicationQuit(){this.Awake();}
	public override void Reset(){this.Awake();}
	public override void Awake(){
		this.alias = this.alias.SetDefault("Attribute");
		this.value.Setup("",this);
	}
}