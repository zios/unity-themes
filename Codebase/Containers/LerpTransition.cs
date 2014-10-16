#pragma warning disable 0169
using Zios;
using System;
using UnityEngine;
[Serializable]
public class LerpTransition{
	public EventBool isAngle;
	public EventBool resetOnChange;
	public EventFloat speed;
	public Transition transition = new Transition();
	protected bool fixedTime;
	protected bool active;
	public void Reset(){
		this.active = false;
	}
	public virtual void Setup(MonoBehaviour script,string eventName,bool isAngle=false){
		EventUtility.Add(script,"Action*End",this.Reset,false);
		if(script is ActionPart){
			ActionPart part = (ActionPart)script;
			this.fixedTime = part.rate == ActionRate.FixedUpdate;
		}
		this.isAngle.Setup(script,eventName+"IsAngle");
		this.isAngle.Set(isAngle);
		this.resetOnChange.Setup(script,eventName+"ResetOnChange");
		this.speed.Setup(script,eventName+"TransitionSpeed");
		this.transition.Setup(script,eventName+"Transition");
	}
	public virtual int Step(int start,int end){
		this.CheckActive();
		return (int)this.Lerp(start,end,this.transition.Tick());
	}
	public virtual float Step(float start,float end){
		this.CheckActive();
		return this.Lerp(start,end,this.transition.Tick());
	}
	public virtual Vector3 Step(Vector3 start,Vector3 end,bool[] useAxes=null){
		if(useAxes == null){useAxes = new bool[3]{true,true,true};}
		this.CheckActive();
		float percent = this.transition.Tick();
		Vector3 current = start;
		if(this.speed != 0){
			float speed = this.speed * percent;
			speed *= this.fixedTime ? Time.fixedDeltaTime : Time.deltaTime;
			Vector3 step = Vector3.MoveTowards(start,end,speed);
			if(useAxes[0]){current.x = step.x;}
			if(useAxes[1]){current.y = step.y;}
			if(useAxes[2]){current.z = step.z;}
		}
		else{
			if(useAxes[0]){current.x = this.Lerp(start.x,end.x,percent);}
			if(useAxes[1]){current.y = this.Lerp(start.y,end.y,percent);}
			if(useAxes[2]){current.z = this.Lerp(start.z,end.z,percent);}
		}
		return current;
	}
	public virtual Vector3 FixedStep(Vector3 current,Vector3 end,float size,bool[] useAxes=null){
		if(useAxes == null){useAxes = new bool[3]{true,true,true};}
		Vector3 value = Vector3.MoveTowards(current,end,size);
		if(useAxes[0]){current.x = value.x;}
		if(useAxes[1]){current.y = value.y;}
		if(useAxes[2]){current.z = value.z;}
		return current;
	}
	private float Lerp(float start,float end,float percent){
		if(this.isAngle){return Mathf.LerpAngle(start,end,percent);}
		else{return Mathf.Lerp(start,end,percent);}
	}
	private void CheckActive(){
		if(!this.active){
			this.active = true;
			this.transition.Reset();
		}
	}
}
[Serializable]
public class LerpVector3 : LerpTransition{
	public bool[] lerpAxes = new bool[3];
	private Vector3 start;
	private Vector3 lastEnd;
	public virtual Vector3 Step(Vector3 current){
		return this.Step(current,current,this.lerpAxes);
	}
	public virtual Vector3 Step(Vector3 start,Vector3 end){
		if(end != this.lastEnd && this.resetOnChange){this.Reset();}
		if(!this.active || this.speed != 0){this.start = start;}
		this.lastEnd = end;
		return base.Step(this.start,end,this.lerpAxes);
	}
}
[Serializable]
public class LerpFloat : LerpTransition{
	private float start;
	private float lastEnd;
	public float Step(float current){
		return this.Step(current,current);
	}
	public override float Step(float start,float end){
		if(end != this.lastEnd && this.resetOnChange){this.Reset();}
		if(!this.active){this.start = start;}
		this.lastEnd = end;
		return base.Step(this.start,end);
	}
}
[Serializable]
public class LerpInt : LerpTransition{
	private int start;
	private int lastEnd;
	public int Step(int current){
		return this.Step(current,current);
	}
	public override int Step(int start,int end){
		if(end != this.lastEnd && this.resetOnChange){this.Reset();}
		if(!this.active){this.start = start;}
		this.lastEnd = end;
		return base.Step(this.start,end);
	}
}
