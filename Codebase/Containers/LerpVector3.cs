using System;
using UnityEngine;
[Serializable]
public class LerpVector3{
	public bool isAngle;
	public Transition transition = new Transition();
	public bool[] axes = new bool[3]{true,true,false};
	private Vector3 start;
	private bool active;
	public void Reset(){
		this.active = false;
	}
	public void Setup(Vector3 current){
		if(!this.active){
			this.transition.Reset();
			this.start = current;
			this.active = true;
		}
	}
	public virtual Vector3 Step(Vector3 current){
		return this.Step(current,current);
	}
	public virtual Vector3 Step(Vector3 current,Vector3 end){
		this.Setup(current);
		float percent = this.transition.Tick();
		if(this.axes[0]){current.x = this.Lerp(this.start.x,end.x,percent);}
		if(this.axes[1]){current.y = this.Lerp(this.start.y,end.y,percent);}
		if(this.axes[2]){current.z = this.Lerp(this.start.z,end.z,percent);}
		return current;
	}
	private float Lerp(float start,float end,float percent){
		if(this.isAngle){return Mathf.LerpAngle(start,end,percent);}
		else{return Mathf.Lerp(start,end,percent);}
	}
}