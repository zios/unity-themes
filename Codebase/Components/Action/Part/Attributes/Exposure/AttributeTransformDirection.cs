#pragma warning disable 0649
#pragma warning disable 0414
using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Transform (Direction)")]
public class AttributeTransformDirection : AttributeExposer{
	private string alias = "Transform";
	private AttributeVector3 directionUp = Vector3.zero;
	private AttributeVector3 directionDown = Vector3.zero;
	private AttributeVector3 directionLeft = Vector3.zero;
	private AttributeVector3 directionRight = Vector3.zero;
	private AttributeVector3 directionForward = Vector3.zero;
	private AttributeVector3 directionBack = Vector3.zero;
	public override void Awake(){
		this.directionUp.getMethod = ()=>this.transform.rotation * Vector3.up;
		this.directionDown.getMethod = ()=>this.transform.rotation * Vector3.down;
		this.directionLeft.getMethod = ()=>this.transform.rotation * Vector3.left;
		this.directionRight.getMethod = ()=>this.transform.rotation * Vector3.right;
		this.directionForward.getMethod = ()=>this.transform.rotation * Vector3.forward;
		this.directionBack.getMethod = ()=>this.transform.rotation * Vector3.back;
	}
}