using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Attributes.Supports.Transition{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions.Convert;
	using Zios.Supports.Transition;
	using Zios.Unity.Extensions.Convert;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	[Serializable]
	public class AttributeTransition : TransitionBase{
		public AttributeFloat time = 0.5f;
		public AttributeFloat speed = 3;
		[HideInInspector] public AttributeFloat delta = 0;
		public AttributeTransition(){}
		public AttributeTransition(AttributeTransition transition){
			this.time.delayedValue = transition.time;
			this.speed.delayedValue = transition.speed;
			this.acceleration = new AnimationCurve().Deserialize(transition.acceleration.Serialize());
			this.deceleration = new AnimationCurve().Deserialize(transition.deceleration.Serialize());
		}
		public AttributeTransition(float time,float speed){
			this.time.delayedValue = time;
			this.speed.delayedValue = speed;
		}
		public override void Setup(string path,UnityObject parent){
			this.path = path;
			this.parent = parent;
			this.time.Setup(path+"/Transition Time",parent);
			this.speed.Setup(path+"/Transition Speed",parent);
			this.delta.Setup(path+"/Transition Delta",parent);
			Events.Register(path+"/Start",parent);
			Events.Register(path+"/End",parent);
		}
		public override float GetTime(){return this.time.Get();}
		public override float GetSpeed(){return this.speed.Get();}
		public override float GetDelta(){return this.delta.Get();}
		public override void SetDelta(float value){this.delta.Set(value);}
		public override void SetTime(float value){this.time.Set(value);}
		public override void SetSpeed(float value){this.speed.Set(value);}
	}
}