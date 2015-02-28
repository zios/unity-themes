using UnityEngine;
using System;
using Zios;
namespace Zios{
    public enum TimerType{After,During}
    [AddComponentMenu("Zios/Component/Action/Timer State")]
    public class TimerState : ActionLink{
	    public TimerType type;
	    public AttributeFloat seconds = 0;
	    [HideInInspector] public AttributeBool isStarted = false;
	    [HideInInspector] public AttributeBool isComplete = false;
	    private float endTime;
	    public override void Awake(){
		    base.Awake();
		    this.seconds.Setup("Seconds",this);
		    this.isStarted.Setup("Is Started",this);
		    this.isComplete.Setup("Is Complete",this);
		    this.endWhileUnusable.Set(this.type == TimerType.After);
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
}