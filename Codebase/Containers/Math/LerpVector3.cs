using Zios;
using System;
using UnityEngine;
[Serializable]
public class LerpVector3 : LerpTransition{
	public bool[] lerpAxes = new bool[3]{true,true,true};
	private Vector3 start;
	private Vector3 lastEnd;
	public virtual Vector3 Step(Vector3 current){
		return this.Step(current,current);
	}
	public virtual Vector3 Step(Vector3 start,Vector3 end){
		if(end != this.lastEnd && this.resetOnChange){this.Reset();}
		if(!this.active || this.speed != 0){this.start = start;}
		this.lastEnd = end;
		this.CheckActive();
		float percent = this.transition.Tick();
		Vector3 current = start;
		if(this.speed != 0){
			float speed = this.speed * percent;
			speed *= this.fixedTime ? Time.fixedDeltaTime : Time.deltaTime;
			Vector3 step = Vector3.MoveTowards(this.start,end,speed);
			//Debug.Log("Start : " + start + " End : " + end + " Change : " + step);
			if(this.lerpAxes[0]){current.x = step.x;}
			if(this.lerpAxes[1]){current.y = step.y;}
			if(this.lerpAxes[2]){current.z = step.z;}
		}
		else{
			if(this.lerpAxes[0]){current.x = this.Lerp(this.start.x,end.x,percent);}
			if(this.lerpAxes[1]){current.y = this.Lerp(this.start.y,end.y,percent);}
			if(this.lerpAxes[2]){current.z = this.Lerp(this.start.z,end.z,percent);}
		}
		return current;
	}
}