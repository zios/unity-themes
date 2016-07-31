#pragma warning disable 0649
#pragma warning disable 0414
using UnityEngine;
namespace Zios.Attributes{
	[AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Rotation)")]
	public class ExposeTransformRotation : DataMonoBehaviour{
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