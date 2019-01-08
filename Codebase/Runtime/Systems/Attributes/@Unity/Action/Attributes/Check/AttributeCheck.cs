using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Attribute Check")]
	public class AttributeCheck : StateBehaviour{
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