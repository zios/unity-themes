using UnityEngine;
namespace Zios.Actions.AttributeComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Attribute/Attribute Check")]
	public class AttributeCheck : StateMonoBehaviour{
		public AttributeBool value = false;
		public override void Awake(){
			base.Awake();
			this.value.Setup("",this);
			this.value.usage = AttributeUsage.Shaped;
		}
		public override void Use(){
			bool active = this.value.Get();
			if(active){base.Use();}
			else{base.End();}
		}
	}
}