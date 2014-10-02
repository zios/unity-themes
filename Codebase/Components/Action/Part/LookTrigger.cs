using Zios;
using UnityEngine;
public enum LookType{LookAt,LookWith}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Look Trigger")]
public class LookTrigger : ActionPart{
	public LookType type;
	public Transform target;
	public bool[] angles = new bool[3]{true,true,false};
	public Timer transition;
	private Vector3 start;
	private Vector3 end;
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		if(this.target == null){return;}
		Vector3 start = this.action.owner.transform.localEulerAngles;
		Vector3 end = start;
		if(this.type == LookType.LookAt){
			this.transform.LookAt(this.target);
			end = this.transform.localEulerAngles;
			this.transform.localEulerAngles = start;
		}
		if(this.type == LookType.LookWith){
			end = this.target.localEulerAngles;
		}
		if(end != this.end){
			this.start = start;
			if(!this.angles[0]){this.end.x = this.start.x;}
			if(!this.angles[1]){this.end.y = this.start.y;}
			if(!this.angles[2]){this.end.z = this.start.z;}
			this.end = end;
		}
		if(this.transition.complete){this.transition.Reset();}
		float percent = this.transition.Tick();
		this.action.owner.transform.localEulerAngles = Vector3.Lerp(this.start,this.end,percent);
		base.Use();
	}
}