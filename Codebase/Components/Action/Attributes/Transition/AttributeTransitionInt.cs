using UnityEngine;
namespace Zios.Actions.TransitionComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Attribute/Transition/Transition Int")]
	public class AttributeTransitionInt: StateMonoBehaviour{
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
			var current = this.transition.Step(target,goal);
			this.target.Set(current);
			base.Use();
		}
	}
}