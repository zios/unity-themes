using Zios;
using System;
using UnityEngine;
[Serializable]
public class LerpFloat : LerpTransition{
	private float start;
	private float lastEnd;
	public float Step(float current){
		return this.Step(current,current);
	}
	public float Step(float start,float end){
		if(end != this.lastEnd && this.resetOnChange){this.Reset();}
		if(!this.active){this.start = start;}
		this.lastEnd = end;
		this.CheckActive();
		return this.Lerp(this.start,end,this.transition.Tick());
	}
}