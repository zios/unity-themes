using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Attributes.Supports.Transition;
	using Zios.State;
	//asm Zios.Supports.Transition;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Transition/Transition Float")]
	public class AttributeTransitionFloat : StateBehaviour{
		public AttributeFloat target = 0;
		public AttributeFloat goal = 0;
		public AttributeTransition transition;
		public override void Awake(){
			base.Awake();
			this.transition.Setup("",this);
			this.target.Setup("Current",this);
			this.goal.Setup("Goal",this);
		}
		public override void Use(){
			var current = this.transition.Step(this.target,this.goal);
			this.target.Set(current);
			base.Use();
		}
	}
}