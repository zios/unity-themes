using UnityEngine;
using System;
using Zios;
public enum TimerEvent{ActionActivate,ActionDeactivate,Custom}
[ExecuteInEditMode][RequireComponent(typeof(Zios.Action))]
[AddComponentMenu("Zios/Component/Action/Part/Timer State")]
public class TimerState : ActionPart{
	public float seconds;
	public TimerEvent triggerEvent;
	public string customEvent;
	private float endTime = -1;
	public void OnValidate(){this.DefaultPriority(5);}
	public override void Start(){
		base.Start();
		string eventName = this.triggerEvent.ToString();
		if(this.customEvent != ""){
			this.triggerEvent = TimerEvent.Custom;
			eventName = this.customEvent;
		}
		Events.Add(eventName,this.Trigger);
	}
	public void Trigger(){
		this.endTime = Time.time + this.seconds;
	}
	public override void Use(){
		if(this.endTime != -1){
			this.inUse = true;
			if(Time.time > this.endTime){
				this.End();
			}
		}
	}
	public override void End(){
		this.endTime = -1;
		this.inUse = false;
	}
} 