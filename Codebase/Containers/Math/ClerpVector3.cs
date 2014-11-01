using Zios;
using System;
using UnityEngine;
[Serializable]
public class ClerpVector3 : LerpVector3{
	public bool[] clampAxes = new bool[3];
	public AttributeVector3 minimum;
	public AttributeVector3 maximum;
	private Vector3 previous;
	public ClerpVector3(){
		this.lerpAxes = new bool[3]{true,true,true};
	}
	public override void Setup(string name="",params MonoBehaviour[] scripts){
		this.minimum.Setup(name+"Minimum",scripts);
		this.maximum.Setup(name+"Maximum",scripts);
		base.Setup(name,scripts);
	}
	public override Vector3 Step(Vector3 current,Vector3 end){
		if(this.clampAxes[0]){end.x = Mathf.Clamp(this.previous.x,this.minimum.x,this.maximum.x);}
		if(this.clampAxes[1]){end.y = Mathf.Clamp(this.previous.y,this.minimum.y,this.maximum.y);}
		if(this.clampAxes[2]){end.z = Mathf.Clamp(this.previous.z,this.minimum.z,this.maximum.z);}
		Vector3 value = base.Step(current,end);
		this.previous = value;
		return value;
	}
}
