using Zios;
using System;
using UnityEngine;
[Serializable]
public class ClerpVector3 : LerpVector3{
	public bool[] clampAxes = new bool[3];
	public EventVector3 minimum;
	public EventVector3 maximum;
	private Vector3 previous;
	public ClerpVector3(){
		this.lerpAxes = new bool[3];
	}
	public override void Setup(MonoBehaviour script,string eventName="",bool isAngle=false){
		this.minimum.Setup(script,eventName+"Minimum");
		this.maximum.Setup(script,eventName+"Maximum");
		base.Setup(script,eventName,isAngle);
	}
	public override Vector3 Step(Vector3 current,Vector3 end){
		if(this.clampAxes[0]){end.x = Mathf.Clamp(this.previous.x,this.minimum.GetX(),this.maximum.GetX());}
		if(this.clampAxes[1]){end.y = Mathf.Clamp(this.previous.y,this.minimum.GetY(),this.maximum.GetY());}
		if(this.clampAxes[2]){end.z = Mathf.Clamp(this.previous.z,this.minimum.GetZ(),this.maximum.GetZ());}
		Vector3 value = base.Step(current,end);
		this.previous = value;
		return value;
	}
}
