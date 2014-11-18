using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Attribute/Attribute Box (String)")]
public class AttributeBoxString : AttributeBox<AttributeString>{
	public override void Reset(){
		this.value = "";
		base.Reset();
	}
	public override void Awake(){
		base.Awake();
	}
}