using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Unity.Components.DataBehaviour;
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.SystemAttributes;
	[AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Scale)")]
	public class ExposeTransformScale : DataBehaviour{
		[Internal] public AttributeVector3 scale = Vector3.zero;
		public override void Awake(){
			this.alias = this.alias.SetDefault("Transform");
			this.autoRename = false;
			base.Awake();
			this.scale.Setup("Scale",this);
			this.scale.getMethod = ()=>this.transform.localScale;
			this.scale.setMethod = value=>this.transform.localScale = value;
		}
	}
}