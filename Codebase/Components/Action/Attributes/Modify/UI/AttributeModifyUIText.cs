using UnityEngine;
namespace Zios.Actions.AttributeComponents{
	using Attributes;
	using UnityEngine.UI;
	[AddComponentMenu("Zios/Component/Action/Attribute/Modify/Modify UI Text")]
	public class AttributeModifyUIText : StateMonoBehaviour{
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