using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Move Towards")]
public class MoveTowards : ActionPart{
	public Target target = new Target();
	public AttributeVector3 goal = Vector3.zero;
	public LerpVector3 travel = new LerpVector3();
	public override void Awake(){
		base.Awake();
		this.DefaultRate("FixedUpdate");
		this.target.Setup("Target",this);
		this.goal.Setup("Goal",this);
		this.travel.Setup("Travel",this);
	}
	public override void End(){
		this.travel.Reset();
		base.End();
	}
	public override void Use(){
		base.Use();
		GameObject target = this.target.Get();
		if(!target.IsNull()){
			Vector3 current = this.travel.Step(target.transform.position,this.goal);	
			Vector3 amount = current-target.transform.position;
			target.Call("Add Move Raw",amount);
		}
	}
}
