using Zios;
using System;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Attribute/Transition/Transition Vector3")]
	public class AttributeTransitionVector3 : AttributeTransition{
		public AttributeFloat speed = 1;
		public AttributeVector3 target = Vector3.zero;
		public AttributeVector3 goal = Vector3.zero;
		[Advanced][ReadOnly] public AttributeVector3 delta = Vector3.zero;
		private Vector3 start;
		private Vector3 lastGoal;
		public override void Awake(){
			base.Awake();
			this.target.Setup("Current",this);
			this.delta.Setup("Delta",this);
			this.goal.Setup("Goal",this);
			this.speed.Setup("Speed",this);
		}
		public override void Use(){
			Vector3 current = this.target.Get();
			Vector3 end = this.goal.Get();
			float speed = this.speed.Get();
			float remainingDistance = current.Distance(end);
			if(speed == -1){
				current = end;
				this.target.Set(end);
			}
			if(current == end){
				if(!this.finished){this.gameObject.CallEvent(this.alias+"/End");}
				this.finished = true;
				this.delta.Set(Vector3.zero);
				this.state = TransitionState.Idle;
				base.Use();
				return;
			}
			if(this.state == TransitionState.Idle){
				this.finished = false;
				this.overallDistance = remainingDistance;
				this.startTime = Time.time + this.transitionSeconds;
				this.gameObject.CallEvent(this.alias+"/Start");
				this.state = this.transitionSeconds <= 0 ? TransitionState.Travel : TransitionState.Acceleration;
				this.start = current;
				this.lastGoal = end;
				speed = 0;
			}
			else if(this.state == TransitionState.Acceleration){
				bool halfTraveled = remainingDistance <= this.overallDistance * 0.5f;
				float transitionIn = (this.transitionSeconds - (this.startTime - Time.time)) / this.transitionSeconds;
				speed *= this.acceleration.Evaluate(transitionIn);
				if(transitionIn >= 1 || halfTraveled){
					this.transitionDistance = this.start.Distance(current);
					this.state = TransitionState.Travel;
					if(halfTraveled){
						this.state = TransitionState.Deceleration;
						this.endTime = Time.time + (transitionIn*this.transitionSeconds);
					}
				}
			}
			else if(this.state == TransitionState.Travel){
				if(this.transitionSeconds > 0 && remainingDistance <= this.transitionDistance){
					float percentRemaining = remainingDistance / this.transitionDistance;
					this.state = TransitionState.Deceleration;
					this.endTime = Time.time + (this.transitionSeconds * percentRemaining);
				}
			}
			else if(this.state == TransitionState.Deceleration){
				float transitionOut = (this.transitionSeconds - (this.endTime - Time.time)) / this.transitionSeconds;
				speed *= this.deceleration.Evaluate(transitionOut);
			}
			if(this.lastGoal != end){
				if(this.state != TransitionState.Travel){this.state = TransitionState.Idle;}
				this.lastGoal = end;
			}
			speed *= this.GetTimeOffset();
			this.delta.Set(current.MoveTowards(end,speed) - current);
			this.target.Set(current + this.delta.Get());
			base.Use();
		}
	}
}