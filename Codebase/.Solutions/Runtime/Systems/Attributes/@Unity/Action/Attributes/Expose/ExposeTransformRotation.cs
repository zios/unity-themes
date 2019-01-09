using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.SystemAttributes;
	using Zios.Unity.Components.DataBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Rotation)")]
	public class ExposeTransformRotation : DataBehaviour{
		[Internal] public AttributeVector3 rotation = Vector3.zero;
		public override void Awake(){
			this.alias = this.alias.SetDefault("Transform");
			this.autoRename = false;
			base.Awake();
			this.rotation.Setup("Rotation",this);
			this.rotation.getMethod = ()=>this.transform.eulerAngles;
			this.rotation.setMethod = value=>this.transform.eulerAngles = value;
		}
	}
}