using UnityEngine;
namespace Zios.Attributes.Actions{
	using UnityEngine.UI;
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Modify/Modify UI Text")]
	public class AttributeModifyUIText : StateBehaviour{
		public Text target;
		public AttributeString value = "Lorem Ipsum";
		public override void Awake(){
			base.Awake();
			this.value.Setup("",this);
		}
		public override void Use(){
			if(!this.target.IsNull()){
				this.target.text = value.Get();
			}
			base.Use();
		}
	}
}