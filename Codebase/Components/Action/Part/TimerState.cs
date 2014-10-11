using UnityEngine;
using System;
using Zios;
public enum TimerType{After,During}
public enum PersistType{Off,UntilComplete,UntilCompleteOrActionEnds}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Timer State")]
public class TimerState : ActionPart{
	public TimerType type;
	public PersistType persist;
	public float seconds;
	public bool scaleByIntensity;
	private float endTime;
	private bool isActive;
	private bool isComplete;
	public override void OnValidate(){
		this.DefaultPriority(5);
		if(this.rate == ActionRate.ActionStart || this.rate == ActionRate.ActionEnd){
			Debug.LogWarning("TimerState ["+this.alias+"] cannot use single-use ActionStart/ActionEnd triggers.");
			this.rate = ActionRate.Default;
		}
		base.OnValidate();
	}
	public void Start(){
		Events.Add("ActionEnd",this.OnActionEnd);
	}
	public override void Use(){
		if(this.isComplete){return;}
		if(!this.isActive){
			float seconds = this.scaleByIntensity ? this.action.intensity * this.seconds : this.seconds;
			this.endTime = Time.time + seconds;
			this.isActive = true;
		}
		bool hasElapsed = Time.time > this.endTime;
		if(this.type == TimerType.After && hasElapsed){
			this.isComplete = true;
			base.Use();
		}
		else if(this.type == TimerType.During){
			base.Use();
			if(hasElapsed){
				this.isComplete = true;
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
		this.isComplete = false;
		this.isActive = false;
		base.End();
	}
} 