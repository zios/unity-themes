using System;
using UnityEngine;
[Serializable]
public class ClerpVector3 : LerpVector3{
	public bool[] clampAxes = new bool[3];
	public Vector3 minimum;
	public Vector3 maximum;
	private Vector3 current;
	public ClerpVector3(){
		this.axes = new bool[3];
	}
	public override Vector3 Step(Vector3 current,Vector3 end){
		if(this.clampAxes[0]){end.x = Mathf.Clamp(this.current.x,this.minimum.x,this.maximum.x);}
		if(this.clampAxes[1]){end.y = Mathf.Clamp(this.current.y,this.minimum.y,this.maximum.y);}
		if(this.clampAxes[2]){end.z = Mathf.Clamp(this.current.z,this.minimum.z,this.maximum.z);}
		Vector3 value = base.Step(current,end);
		this.current = value;
		return value;
	}
}