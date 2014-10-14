using UnityEngine;
using System;
[Serializable]
public class Transition{
	public float duration = 0.5f;
	public AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
	[NonSerialized] public bool complete = true;
	[NonSerialized] public float endTime;
	public void Reset(){
		float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
		this.endTime = time + this.duration;
	}
	public void End(){
		this.endTime = 0;
		this.complete = true;
	}
	public float Tick(){
		float startTime = this.endTime - this.duration;
		float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
		float elapsed = this.duration <= 0 ? 1 :(time-startTime)/this.duration;
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