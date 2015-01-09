using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Rotate/Towards/Rotate Towards Target")]
public class RotateTowardsTarget : ActionLink{
	public AttributeGameObject source = new AttributeGameObject();
	public AttributeGameObject goal = new AttributeGameObject();
	public LerpVector3 rotation = new LerpVector3();
	public override void Awake(){
		base.Awake();
		this.source.Setup("Source",this);
		this.goal.Setup("Goal Target",this);
		this.rotation.Setup("Rotation Target",this);
		this.rotation.isAngle.Set(true);
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform goal = this.goal.Get().transform;
		Vector3 start = source.localEulerAngles;
		source.LookAt(goal.position);
		Vector3 end = source.localEulerAngles;
		source.localEulerAngles = this.rotation.Step(start,end);
		base.Use();
	}
}
