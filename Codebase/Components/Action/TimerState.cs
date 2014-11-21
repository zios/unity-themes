using UnityEngine;
using System;
using Zios;
public enum TimerType{After,During}
public enum PersistType{Off,UntilComplete,UntilCompleteOrActionEnds}
[AddComponentMenu("Zios/Component/Action/Timer State")]
public class TimerState : ActionPart{
	public TimerType type;
	public PersistType persist;
	public AttributeFloat seconds = 0;
	private float endTime;
	private AttributeBool isActive = false;
	private AttributeBool isComplete = false;
	public override void Awake(){
		base.Awake();
		this.DefaultPriority(5);
		this.seconds.Setup("Seconds",this);
		this.isActive.Setup("Is Active",this);
		this.isComplete.Setup("Is Complete",this);
		Events.Add("Action End",this.OnActionEnd);
	}
	public override void Use(){
		if(this.isComplete){return;}
		if(!this.isActive){
			float seconds = this.seconds.Get();
			this.endTime = Time.time + seconds;
			this.isActive.Set(true);
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
	public void OnActionEnd(){
		if(this.type == TimerType.During || this.persist == PersistType.UntilCompleteOrActionEnds){
			this.ForceEnd();
		}
	}
	public override void End(){
		if(!this.isComplete && this.persist != PersistType.Off){
			this.Use();
			return;
		}
		this.ForceEnd();
	}
	public override void ForceEnd(){
		this.isComplete.Set(false);
		this.isActive.Set(false);
		base.End();
	}
} 
