using UnityEngine;
namespace Zios.Actions.TransitionComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Attribute/Transition/Transition Float")]
	public class AttributeTransitionFloat : StateMonoBehaviour{
		public AttributeFloat target = 0;
		public AttributeFloat goal = 0;
		public Transition transition;
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