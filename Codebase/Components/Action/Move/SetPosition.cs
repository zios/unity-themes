using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Move/Set Position")]
public class SetPosition : ActionPart{
	public AttributeGameObject target = new AttributeGameObject();
	public AttributeVector3 position = Vector3.zero;
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.position.Setup("Position",this);
	}
	public override void Use(){
		this.target.Get().transform.position = this.position.Get();
		base.Use();
	}
}
