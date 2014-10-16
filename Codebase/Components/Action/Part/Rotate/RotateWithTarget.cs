using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Rotate With (Target)")]
public class RotateWithTarget : ActionPart{
	public Target source = new Target();
	public Target target = new Target();
	public ClerpVector3 angles = new ClerpVector3();
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.source.Update(this);
		this.target.Update(this);
	}
	public void Start(){
		this.source.Setup(this);
		this.target.Setup(this);
		this.angles.Setup(this,"Vector",true);
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform target = this.target.Get().transform;
		Vector3 current = source.localEulerAngles;
		Vector3 goal = target.localEulerAngles;
		source.localEulerAngles = this.angles.Step(current,goal);
		base.Use();
	}
}
