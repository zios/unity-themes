#pragma warning disable 0169
using Zios;
using System;
using UnityEngine;
public class LerpTransition{
	public AttributeBool isAngle = true;
	public AttributeBool resetOnChange;
	public AttributeFloat speed;
	public Transition transition = new Transition();
	protected bool fixedTime;
	protected bool active;
	public void Reset(){
		this.active = false;
	}
	public virtual void Setup(string name,params MonoBehaviour[] scripts){
		foreach(MonoBehaviour script in scripts){
			if(script is ActionPart){
				ActionPart part = (ActionPart)script;
				this.fixedTime = part.rate == ActionRate.FixedUpdate;
			}
		}
		this.isAngle.Setup(name+"IsAngle",scripts);
		this.resetOnChange.Setup(name+"ResetOnChange",scripts);
		this.speed.Setup(name+"TransitionSpeed",scripts);
		this.transition.Setup(name+"Transition",scripts);
	}
	public virtual Vector3 FixedStep(Vector3 current,Vector3 end,float size,bool[] useAxes=null){
		if(useAxes == null){useAxes = new bool[3]{true,true,true};}
		Vector3 value = Vector3.MoveTowards(current,end,size);
		if(useAxes[0]){current.x = value.x;}
		if(useAxes[1]){current.y = value.y;}
		if(useAxes[2]){current.z = value.z;}
		return current;
	}
	protected float Lerp(float start,float end,float percent){
		if(this.isAngle){return Mathf.LerpAngle(start,end,percent);}
		else{return Mathf.Lerp(start,end,percent);}
	}
	protected void CheckActive(){
		if(!this.active){
			this.active = true;
			this.transition.Reset();
		}
	}
}