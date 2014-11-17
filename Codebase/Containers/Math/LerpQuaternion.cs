using Zios;
using System;
using UnityEngine;
[Serializable]
public class LerpQuaternion : LerpTransition{
	private Quaternion start;
	private Quaternion lastEnd;
	private string path;
	private Component parent;
	public override void Setup(string path,Component parent){
		this.path = path.AddRoot(parent);
		this.parent = parent;
		this.parent.gameObject.Register(this.path+"/Transition/End");
		this.parent.gameObject.Register(this.path+"/Transition/Start");
		base.Setup(this.path,parent);
	}
	public virtual Quaternion Step(Quaternion current){
		return this.Step(current,current);
	}
	public virtual Quaternion Step(Quaternion start,Quaternion end){
		if(end != this.lastEnd && this.isResetOnChange){this.Reset();}
		if(!this.active || this.speed != 0){this.start = start;}
		this.lastEnd = end;
		this.CheckActive();
		float percent = this.transition.Tick();
		Quaternion current = start;
		if(this.speed != 0){
			float speed = this.speed * percent;
			speed *= this.fixedTime ? Time.fixedDeltaTime : Time.deltaTime;
			Quaternion step = Quaternion.RotateTowards(this.start,end,speed);
			if(this.parent != null){
				if(step == end && current != end){
					this.parent.gameObject.Call(this.path+"/Transition/End");
				}
				if(step != end && current == end){
					this.parent.gameObject.Call(this.path+"/Transition/Start");
				}
			}
			current = step;
		}
		else{
			current = Quaternion.Slerp(this.start,end,percent);
		}
		return current;
	}
}
