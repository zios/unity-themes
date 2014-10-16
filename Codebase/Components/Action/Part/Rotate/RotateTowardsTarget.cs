using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Rotate Towards (Target)")]
public class RotateTowardsTarget : ActionPart{
	public Target source = new Target();
	public Target target = new Target();
	public Vector3 offset;
	public ClerpVector3 angles = new ClerpVector3();
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.source.Update(this);
		this.target.Update(this);
	}
	public void Start(){
		this.source.Setup(this,"Source");
		this.target.Setup(this);
		this.angles.Setup(this,"Vector",true);
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform target = this.target.Get().transform;
		Vector3 offset = target.right * this.offset.x;
		offset += target.up * this.offset.y;
		offset += target.forward * this.offset.z;
		Vector3 current = source.localEulerAngles;
		source.LookAt(target.position + offset);
		Vector3 goal = source.localEulerAngles;
		source.localEulerAngles = this.angles.Step(current,goal);
		base.Use();
	}
}
