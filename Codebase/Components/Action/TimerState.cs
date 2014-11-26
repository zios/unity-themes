using UnityEngine;
using System;
using Zios;
public enum TimerType{After,During}
[AddComponentMenu("Zios/Component/Action/Timer State")]
public class TimerState : ActionPart{
	public TimerType type;
	public AttributeFloat seconds = 0;
	private float endTime;
	private AttributeBool isStarted = false;
	private AttributeBool isComplete = false;
	public override void Awake(){
		base.Awake();
		this.seconds.Setup("Seconds",this);
		this.isStarted.Setup("Is Started",this);
		this.isComplete.Setup("Is Complete",this);
	}
	public override void Use(){
		if(this.isComplete){return;}
		if(!this.isStarted){
			float seconds = this.seconds.Get();
			this.endTime = Time.time + seconds;
			this.isStarted.Set(true);
		}
		bool hasElapsed = Time.time > this.endTime;
		if(this.type == TimerType.After && hasElapsed){
			this.isComplete.Set(true);
			base.Use();
		}
		else if(this.type == TimerType.During){
			base.Use();
			if(hasElapsed){
				this.isComplete.Set(true);
				base.End();
			}
		}
	}
	public override void End(){
		this.isComplete.Set(false);
		this.isStarted.Set(false);
		base.End();
	}
} 
