using UnityEngine;
namespace Zios.Actions.AnimationComponents{
	using Animations;
	using Attributes;
	using Events;
	[AddComponentMenu("Zios/Component/Action/General/Play Animation (3D)")]
	public class Play3DAnimation : StateMonoBehaviour{
		public AttributeString animationName = "";
	   [Advanced] public AttributeFloat speed = -1;
	   [Advanced] public AttributeFloat weight = -1;
		public AttributeGameObject target = new AttributeGameObject();
		public override void Awake(){
			base.Awake();
			this.animationName.Setup("Animation Name",this);
			this.speed.Setup("Speed",this);
			this.weight.Setup("Weight",this);
			this.target.Setup("Target",this);
			this.AddDependent<AnimationController>(this.target);
		}
		public override void Use(){
			base.Use();
			string name = this.animationName.Get();
			foreach(var target in this.target){
				if(this.speed != -1){target.CallEvent("Set Animation Speed",name,this.speed.Get());}
				if(this.weight != -1){target.CallEvent("Set Animation Weight",name,this.weight.Get());}
				target.CallEvent("Play Animation",name);
			}
		}
		public override void End(){
			base.End();
			foreach(var target in this.target){
				target.CallEvent("Stop Animation",this.animationName.Get());
			}
		}
	}
}