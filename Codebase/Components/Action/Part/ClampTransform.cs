using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Clamp (Transform)")]
public class ClampTransform : ActionPart{
	public Transform target;
	public float[] minPosition = new float[3];
	public float[] maxPosition = new float[3];
	public float[] minRotation = new float[3];
	public float[] maxRotation = new float[3];
	public float[] minScale = new float[3];
	public float[] maxScale = new float[3];
	public bool[] positionClamp = new bool[3];
	public bool[] rotationClamp = new bool[3];
	public bool[] scaleClamp = new bool[3];
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public Vector3 Clamp(Vector3 current,bool[] state,float[] min,float[] max){
		if(state[0]){current[0] = Mathf.Clamp(current[0],min[0],max[0]);}
		if(state[1]){current[1] = Mathf.Clamp(current[1],min[1],max[1]);}
		if(state[2]){current[2] = Mathf.Clamp(current[2],min[2],max[2]);}
		return current;
	}
	public override void Use(){
		if(this.target == null){
			this.target = this.action.owner.transform;
		}
		this.target.position = this.Clamp(this.target.position,this.positionClamp,this.minPosition,this.maxPosition);
		this.target.localEulerAngles = this.Clamp(this.target.localEulerAngles,this.rotationClamp,this.minRotation,this.maxRotation);
		this.target.localScale = this.Clamp(this.target.localScale,this.scaleClamp,this.minScale,this.maxScale);
		base.Use();
	}
}