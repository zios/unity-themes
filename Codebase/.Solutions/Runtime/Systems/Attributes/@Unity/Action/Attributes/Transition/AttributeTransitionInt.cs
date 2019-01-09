using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Attributes.Supports.Transition;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Attribute/Transition/Transition Int")]
	public class AttributeTransitionInt: StateBehaviour{
		public AttributeInt target = 0;
		public AttributeInt goal = 0;
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