using Zios;
using UnityEngine;
using System;
namespace Zios{
    [Serializable]
    public class Transition{
	    public AttributeFloat duration = 0.5f;
	    public AttributeFloat delayStart = 0;
	    public AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
	    [NonSerialized] public bool complete = true;
	    [NonSerialized] public float endTime;
	    [NonSerialized] public float startTime;
	    public void Reset(){
		    float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
		    this.startTime = time + this.delayStart;
		    this.endTime = time + this.duration + this.delayStart;
	    }
	    public virtual void Setup(string path,Component parent){
		    path = (parent.GetAlias() + "/" + path).Trim("/");
		    this.duration.Setup(path+"/Duration",parent);
		    this.delayStart.Setup(path+"/Delay Start",parent);
		    this.duration.locked = true;
		    this.delayStart.locked = true;
	    }
	    public void End(){
		    this.endTime = 0;
		    this.complete = true;
	    }
	    public float Tick(){
		    float startTime = this.endTime - this.duration;
		    float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
		    if(time < this.startTime){return 0;}
		    float elapsed = this.duration <= 0 ? 1 : (time-startTime)/this.duration;
		    this.complete = time >= endTime;
		    return this.curve.Evaluate(elapsed);
	    }
	    public Transition Copy(){
		    Transition copy = new Transition();
		    copy.duration = this.duration;
		    copy.curve = this.curve;
		    return copy;
	    }
    }
}