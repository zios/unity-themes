using UnityEngine;
using System;
[AddComponentMenu("Zios/Component/General/Follow")]
public class Follow : StateMonoBehaviour{
	public Vector3 targetPosition;
	public Vector3 targetOffset;
	public Vector2 orbitAngles;
	public Target target;
	public Transition transition;
	[NonSerialized] public float percent;
	[NonSerialized] public GameObject lastTarget;
	[NonSerialized] public Vector3 lastTargetPosition;
	[NonSerialized] public Vector2 lastOrbitAngles;
	[NonSerialized] public Vector3 lastTargetOffset;
	[NonSerialized] public Vector2 angleStart;
	[NonSerialized] public Vector3 offsetStart;
	[NonSerialized] public Vector2 lastLerpAngles;
	[NonSerialized] public Vector3 lastLerpOffset;
	public void Start(){
		this.target.Setup(this);
		this.target.DefaultTarget(this.gameObject);
		this.lastLerpAngles = this.orbitAngles;
		this.lastLerpOffset = this.targetOffset;
		this.Update();
		this.transition.End();
		Events.AddGet("GetOrbitAngles",this.OnGetOrbitAngles);
		Events.AddGet("GetTargetOffset",this.OnGetTargetOffset);
		Events.AddGet("GetTargetPosition",this.OnGetTargetPosition);
		Events.Add("SetOrbitAngles",(MethodVector2)this.OnSetOrbitAngles);
		Events.Add("SetTargetOffset",(MethodVector3)this.OnSetTargetOffset);
		Events.Add("SetTargetPosition",(MethodVector3)this.OnSetTargetPosition);
		Events.Add("AddOrbitAngles",(MethodVector2)this.OnAddOrbitAngles);
		Events.Add("AddTargetOffset",(MethodVector3)this.OnAddTargetOffset);
	}
	public object OnGetOrbitAngles(){return this.orbitAngles;}
	public object OnGetTargetOffset(){return this.targetOffset;}
	public object OnGetTargetPosition(){return this.targetPosition;}
	public void OnSetOrbitAngles(Vector2 angles){this.orbitAngles = angles;}
	public void OnSetTargetOffset(Vector3 offset){this.targetOffset = offset;}
	public void OnSetTargetPosition(Vector3 position){this.targetPosition = position;}
	public void OnAddOrbitAngles(Vector2 angles){this.orbitAngles += angles;}
	public void OnAddTargetOffset(Vector3 offset){this.targetOffset += offset;}
	public void Update(){
		GameObject target = this.target.Get();
		if(target != null){
			this.targetPosition = target.transform.position;
		}
		bool targetChanged = target != this.lastTarget;
		bool targetPositionChanged = this.targetPosition != this.lastTargetPosition;
		bool anglesChanged = this.orbitAngles != this.lastOrbitAngles;
		bool offsetChanged = this.targetOffset != this.lastTargetOffset;
		if(targetChanged || targetPositionChanged || offsetChanged || anglesChanged){
			this.lastTargetPosition = this.targetPosition;
			this.lastTarget = target;
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
		this.lastLerpAngles = this.lastLerpAngles.LerpAngle(this.angleStart,this.orbitAngles,percent);
		Quaternion rotation = Quaternion.Euler(this.lastLerpAngles[1],this.lastLerpAngles[0],0);
		this.transform.position = rotation * range + this.targetPosition;
	}
	public void LateUpdate(){
		this.lastLerpOffset = Vector3.Lerp(this.offsetStart,this.targetOffset,percent);
		Quaternion rotation = Quaternion.Euler(this.lastLerpAngles[1],this.lastLerpAngles[0],0);
		this.transform.position = rotation * this.lastLerpOffset + this.targetPosition;
	}
}

