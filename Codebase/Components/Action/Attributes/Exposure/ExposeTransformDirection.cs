#pragma warning disable 0649
#pragma warning disable 0414
using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Direction)")]
public class ExposeTransformDirection : DataMonoBehaviour{
	[HideInInspector] public AttributeVector3 directionUp = Vector3.zero;
	[HideInInspector] public AttributeVector3 directionDown = Vector3.zero;
	[HideInInspector] public AttributeVector3 directionLeft = Vector3.zero;
	[HideInInspector] public AttributeVector3 directionRight = Vector3.zero;
	[HideInInspector] public AttributeVector3 directionForward = Vector3.zero;
	[HideInInspector] public AttributeVector3 directionBack = Vector3.zero;
	public override void Awake(){
		this.alias = this.alias.SetDefault("Transform");
		base.Awake();
		this.directionUp.Setup("Direction/Up",this);
		this.directionDown.Setup("Direction/Down",this);
		this.directionLeft.Setup("Direction/Left",this);
		this.directionRight.Setup("Direction/Right",this);
		this.directionForward.Setup("Direction/Forward",this);
		this.directionBack.Setup("Direction/Back",this);
		this.directionUp.getMethod = ()=>this.transform.rotation * Vector3.up;
		this.directionDown.getMethod = ()=>this.transform.rotation * Vector3.down;
		this.directionLeft.getMethod = ()=>this.transform.rotation * Vector3.left;
		this.directionRight.getMethod = ()=>this.transform.rotation * Vector3.right;
		this.directionForward.getMethod = ()=>this.transform.rotation * Vector3.forward;
		this.directionBack.getMethod = ()=>this.transform.rotation * Vector3.back;
	}
}
