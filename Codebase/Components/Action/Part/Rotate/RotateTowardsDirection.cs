using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Rotate Towards (Direction)")]
public class RotateTowardsDirection : ActionPart{
	public EventVector3 direction;
	public Target source = new Target();
	public LerpVector3 rotation = new LerpVector3();
	private Vector3 lastDirection;
	private Vector3 current;
	public override void OnValidate(){
		this.DefaultRate("FixedUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.source.Update(this);
	}
	public void Start(){
		this.source.Setup(this);
		this.rotation.Setup(this,"Rotation");
		this.direction.Setup(this,"RotateDirection");
	}
	public override void Use(){
		if(this.lastDirection != this.direction){
			this.rotation.Reset();
		}
		Transform transform = this.source.Get().transform;
		Vector3 current = transform.rotation * Vector3.forward;
		Vector3 goal = this.rotation.Step(current,this.direction);
		//this.current = goal;
		if(goal != Vector3.zero){
			transform.rotation = Quaternion.LookRotation(goal);
		}
		this.lastDirection = this.direction;
		base.Use();
	}
}
