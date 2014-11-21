using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Attribute/Box/Box Int")]
public class AttributeBoxInt : AttributeBox<AttributeInt>{
	public override void Reset(){
		this.value = 0;
		base.Reset();
	}
	public override void Awake(){
		base.Awake();
	}
}