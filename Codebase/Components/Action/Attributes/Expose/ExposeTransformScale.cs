#pragma warning disable 0649
#pragma warning disable 0414
using UnityEngine;
namespace Zios.Attributes{
	[AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Scale)")]
	public class ExposeTransformScale : DataMonoBehaviour{
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