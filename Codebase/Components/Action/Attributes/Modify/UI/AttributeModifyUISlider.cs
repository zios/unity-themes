using UnityEngine;
namespace Zios.Actions.AttributeComponents{
	using Attributes;
	using UnityEngine.UI;
	[AddComponentMenu("Zios/Component/Action/Attribute/Modify/Modify UI Slider")]
	public class AttributeModifyUISlider : StateMonoBehaviour{
		public Slider target;
		public AttributeFloat minimum = 0;
		public AttributeFloat maximum = 1;
		public AttributeFloat current = 0.5f;
		public override void Awake(){
			base.Awake();
			this.minimum.Setup("Minimum",this);
			this.maximum.Setup("Maximum",this);
			this.current.Setup("Current",this);
		}
		public override void Use(){
			if(!this.target.IsNull()){
				this.target.minValue = this.minimum.Get();
				this.target.maxValue = this.maximum.Get();
				this.target.value = this.current.Get();
			}
			base.Use();
		}
	}
}