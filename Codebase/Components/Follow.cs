using UnityEngine;
using System;
[AddComponentMenu("Zios/Component/General/Follow")]
public class Follow : StateMonoBehaviour{
	public Transform target;
	public Vector3 targetPosition;
	public Vector3 targetOffset;
	public Vector2 orbitAngles;
	public Timer transition;
	[NonSerialized] public float percent;
	[NonSerialized] public Transform lastTarget;
	[NonSerialized] public Vector3 lastTargetPosition;
	[NonSerialized] public Vector2 lastOrbitAngles;
	[NonSerialized] public Vector3 lastTargetOffset;
	[NonSerialized] public Vector2 angleStart;
	[NonSerialized] public Vector3 offsetStart;
	[NonSerialized] public Vector2 lastLerpAngles;
	[NonSerialized] public Vector3 lastLerpOffset;
	public void Start(){
		this.lastLerpAngles = this.orbitAngles;
		this.lastLerpOffset = this.targetOffset;
		this.Update();
		this.transition.End();
		Events.Add("SetTarget",(MethodObject)this.OnSetTarget);
		Events.Add("SetOrbitAngles",(MethodVector2)this.OnSetOrbitAngles);
		Events.Add("SetTargetOffset",(MethodVector3)this.OnSetTargetOffset);
		Events.Add("SetTargetPosition",(MethodVector3)this.OnSetTargetPosition);
		Events.Add("AddOrbitAngles",(MethodVector2)this.OnAddOrbitAngles);
		Events.Add("AddTargetOffset",(MethodVector3)this.OnAddTargetOffset);
	}
	public void OnSetTarget(object target){
		if(target is GameObject){this.target = ((GameObject)target).transform;}
		if(target is Transform){this.target = ((Transform)target);}
	}
	public void OnSetOrbitAngles(Vector2 angles){this.orbitAngles = angles;}
	public void OnSetTargetOffset(Vector3 offset){this.targetOffset = offset;}
	public void OnSetTargetPosition(Vector3 position){this.targetPosition = position;}
	public void OnAddOrbitAngles(Vector2 angles){this.orbitAngles += angles;}
	public void OnAddTargetOffset(Vector3 offset){this.targetOffset += offset;}
	public void Update(){
		//this.orbitAngles = this.WrapAngles(this.orbitAngles);
		if(this.target){this.targetPosition = this.target.position;}
		bool targetChanged = this.target != this.lastTarget;
		bool targetPositionChanged = this.targetPosition != this.lastTargetPosition;
		bool anglesChanged = this.orbitAngles != this.lastOrbitAngles;
		bool offsetChanged = this.targetOffset != this.lastTargetOffset;
		if(targetChanged || targetPositionChanged || offsetChanged || anglesChanged){
			this.lastTargetPosition = this.targetPosition;
			this.lastTarget = this.target;
			this.lastOrbitAngles = this.orbitAngles;
			this.lastTargetOffset = this.targetOffset;
			this.angleStart = this.lastLerpAngles;
			this.offsetStart = this.lastLerpOffset;
			if(this.transition.complete){
				this.transition.Reset();
			}
		}
		this.percent = this.transition.Tick();
		Vector3 range = new Vector3(0,0,this.targetOffset[2]);
		Vector2 shortestAngles = new Vector2(0,0);
		for(int index=0;index<2;index++){
			if(this.orbitAngles[index] < this.angleStart[index]){
				float distanceA = this.orbitAngles[index] + (360 - this.angleStart[index]);
				float distanceB = this.angleStart[index] - this.orbitAngles[index];
				shortestAngles[index] = distanceA <= distanceB ? (360 + this.orbitAngles[index]) : this.orbitAngles[index];
			}
			else{
				float distanceA = this.angleStart[index] + (360 - this.orbitAngles[index]);
				float distanceB = this.orbitAngles[index] - this.angleStart[index];
				shortestAngles[index] = distanceA <= distanceB ? (this.angleStart[index] - distanceA) : this.orbitAngles[index];
			}
		}
		this.lastLerpAngles = Vector2.Lerp(this.angleStart,shortestAngles,percent);
		Quaternion rotation = Quaternion.Euler(this.lastLerpAngles[1],this.lastLerpAngles[0],0);
		this.transform.position = rotation * range + this.targetPosition;
		//this.lastLerpAngles = this.WrapAngles(this.lastLerpAngles);
	}
	public void LateUpdate(){
		this.lastLerpOffset = Vector3.Lerp(this.offsetStart,this.targetOffset,percent);
		Quaternion rotation = Quaternion.Euler(this.lastLerpAngles[1],this.lastLerpAngles[0],0);
		this.transform.position = rotation * this.lastLerpOffset + this.targetPosition;
	}
	public Vector2 WrapAngles(Vector2 cap){
		Vector2 wrapped = new Vector2(0,0);
		for(int index=0;index<2;index++){
			wrapped[index] = cap[index] % 360;
			if(wrapped[index] < 0){
				wrapped[index] = 360 - (-wrapped[index]);
			}
		}
		return wrapped;
	}
}

