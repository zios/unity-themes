using Zios;
using UnityEngine;
public enum PositionMode{World,Local}
[AddComponentMenu("Zios/Component/Action/Move/Set Position")]
public class SetPosition : ActionLink{
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
