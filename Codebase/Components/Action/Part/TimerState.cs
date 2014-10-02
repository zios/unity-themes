using UnityEngine;
using System;
using Zios;
public enum TimerType{After,During}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Timer State")]
public class TimerState : ActionPart{
	public float seconds;
	public TimerType type;
	private float endTime = -1;
	public override void OnValidate(){
		this.DefaultPriority(5);
		base.OnValidate();
	}
	public void Start(){
		this.SetupEvents(this);
	}
	public override void Use(){
		if(this.endTime == -1){
			this.endTime = Time.time + this.seconds;
		}
		bool hasElapsed = Time.time > this.endTime;
		if(this.type == TimerType.After && hasElapsed){
			base.Use();
		}
		else if(this.type == TimerType.During && this.endTime != -2){
			base.Use();
			if(hasElapsed){
				base.End();
				this.endTime = -2;
			}
		}
	}
	public override void OnActionEnd(){
		this.endTime = -1;
		base.End();
	}
	public override void End(){
		this.OnActionEnd();
	}
} 