using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Move/Set Position")]
public enum PositionMode{World,Local}
public class SetPosition : ActionPart{
	public PositionMode mode;
	public AttributeGameObject target = new AttributeGameObject();
	public AttributeVector3 position = Vector3.zero;
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.position.Setup("Position",this);
	}
	public override void Use(){
		Vector3 position = this.position.Get();
		Transform target = this.target.Get().transform;
		if(this.mode == PositionMode.World){target.position = position;}
		if(this.mode == PositionMode.Local){target.localPosition = position;}
		base.Use();
	}
}
