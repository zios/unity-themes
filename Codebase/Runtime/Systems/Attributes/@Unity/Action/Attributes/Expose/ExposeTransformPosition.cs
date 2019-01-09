using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.SystemAttributes;
	using Zios.Unity.Components.DataBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Position)")]
	public class ExposeTransformPosition : DataBehaviour{
		[Internal] public AttributeVector3 position = Vector3.zero;
		public override void Awake(){
			this.alias = this.alias.SetDefault("Transform");
			this.autoRename = false;
			base.Awake();
			this.position.Setup("Position",this);
			this.position.getMethod = ()=>this.transform.position;
			this.position.setMethod = value=>this.transform.position = value;
		}
	}
}