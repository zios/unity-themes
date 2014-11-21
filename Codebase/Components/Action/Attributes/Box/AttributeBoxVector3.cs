using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Attribute/Box/Box Vector3")]
public class AttributeBoxVector3 : AttributeBox<AttributeVector3>{
	public override void Reset(){
		this.value = Vector3.zero;
		base.Reset();
	}
	public override void Awake(){
		base.Awake();
	}
}