using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.SystemAttributes;
	using Zios.Unity.Components.DataBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Direction)")]
	public class ExposeTransformDirection : DataBehaviour{
		[Internal] public AttributeVector3 directionUp = Vector3.zero;
		[Internal] public AttributeVector3 directionDown = Vector3.zero;
		[Internal] public AttributeVector3 directionLeft = Vector3.zero;
		[Internal] public AttributeVector3 directionRight = Vector3.zero;
		[Internal] public AttributeVector3 directionForward = Vector3.zero;
		[Internal] public AttributeVector3 directionBack = Vector3.zero;
		public override void Awake(){
			this.alias = this.alias.SetDefault("Transform");
			this.autoRename = false;
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
}