using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Attribute/Box/Box Float")]
public class AttributeBoxFloat : AttributeBox<AttributeFloat>{
	public override void Reset(){
		this.value = 0;
		base.Reset();
	}
	public override void Awake(){
		base.Awake();
	}
}