using UnityEngine;
using System;
[AddComponentMenu("Zios/Component/General/Watch")]
public class Watch : StateMonoBehaviour{
	public Vector3 targetPosition;
	public Vector3 targetOffset;
	public Target target = new Target();
	public Transition transition;
	[NonSerialized] public Quaternion rotationStart;
	[NonSerialized] public Quaternion rotationEnd;
	[NonSerialized] public Vector3 lastPosition;
	[NonSerialized] public Vector3 lastTargetPosition;
	[NonSerialized] public Vector3 lastTargetOffset;
	public void Start(){
		this.target.Setup(this);
		this.target.DefaultTarget(this.gameObject);
		this.LateUpdate();
		this.transform.LookAt(this.targetPosition);
		this.transition.End();
	}
	public void LateUpdate(){
		if(this.target.direct != null){this.targetPosition = this.target.Get().transform.position;}
		bool targetChanged = this.targetPosition != this.lastTargetPosition;
		bool selfChanged = this.transform.position != this.lastPosition;
		bool offsetChanged = this.targetOffset != this.lastTargetOffset;
		if(targetChanged || selfChanged || offsetChanged){
			this.lastPosition = this.transform.position;
			this.lastTargetPosition = this.targetPosition;
			this.lastTargetOffset = this.targetOffset;
			this.rotationStart = this.transform.rotation;
			this.transform.LookAt(this.targetPosition + this.targetOffset);
			this.rotationEnd = this.transform.rotation;
			this.transform.rotation = this.rotationStart;
			if(this.transition.complete){
				this.transition.Reset();
			}
		}
		float percent = this.transition.Tick();
		this.transform.rotation = Quaternion.Lerp(this.rotationStart,this.rotationEnd,percent);
	}
}