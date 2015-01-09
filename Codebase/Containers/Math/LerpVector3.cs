using Zios;
using System;
using UnityEngine;
[Serializable]
public class LerpVector3 : LerpTransition{
	public ListBool lerpAxes = new ListBool{true,true,true};
	public float endProximity;
	private Vector3? lastStart;
	private Vector3? lastEnd;
	public override void Setup(string path,Component parent){
		base.Setup(path,parent);
		this.parent.gameObject.Register(this.path+"/Transition/End");
		this.parent.gameObject.Register(this.path+"/Transition/Start");
	}
	public virtual Vector3 Step(Vector3 current){
		return this.Step(current,current);
	}
	public virtual Vector3 Step(Vector3 start,Vector3 end){
		if(!this.lerpAxes[0]){end.x = start.x;}
		if(!this.lerpAxes[1]){end.y = start.y;}
		if(!this.lerpAxes[2]){end.z = start.z;}
		float distance = Vector3.Distance(start,end);
		if(distance <= this.endProximity){
			if(this.active){
				this.parent.gameObject.Call(this.path+"/Transition/End");
				this.active = false;
			}
			return start;
		}
		if(this.isResetOnChange){
			if(this.lastEnd != end){
				this.Reset();
				this.active = false;
			}
		}
		if(!this.active){
			this.transition.Reset();
			this.parent.gameObject.Call(this.path+"/Transition/Start");
			this.lastStart = start;
			this.lastEnd = end;
			this.active = true;
		}
		float percent = this.transition.Tick();
		Vector3 current = start;
		if(this.speed != 0){
			float speed = this.speed * percent;
			speed *= this.fixedTime ? Time.fixedDeltaTime : Time.deltaTime;
			current = Vector3.MoveTowards(current,end,speed);
		}
		else{
			Vector3 lastStart = (Vector3)this.lastStart;
			if(this.lerpAxes[0]){current.x = this.Lerp(lastStart.x,end.x,percent);}
			if(this.lerpAxes[1]){current.y = this.Lerp(lastStart.y,end.y,percent);}
			if(this.lerpAxes[2]){current.z = this.Lerp(lastStart.z,end.z,percent);}
		}
		return current;
	}
}
