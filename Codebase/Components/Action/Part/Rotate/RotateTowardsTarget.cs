using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Rotate Towards (Target)")]
public class RotateTowardsTarget : ActionPart{
	public Target source = new Target();
	public Target target = new Target();
	public Vector3 offset;
	public OffsetType offsetType;
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
	public Vector3 AdjustVector(Vector3 value){
		Vector3 adjusted = value;
		if(this.offsetType == OffsetType.Relative){
			Transform target = this.target.direct.transform;
			adjusted = target.right * value.x;
			adjusted += target.up * value.y;
			adjusted += target.forward * value.z;
		}
		return adjusted;
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform target = this.target.Get().transform;
		Vector3 offset = this.AdjustVector(this.offset);
		Vector3 current = source.localEulerAngles;
		source.LookAt(target.position + offset);
		Vector3 goal = source.localEulerAngles;
		source.localEulerAngles = this.angles.Step(current,goal);
		base.Use();
	}
}
