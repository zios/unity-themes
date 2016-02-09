using UnityEngine;
using System;
namespace Zios.Actions.TransitionComponents{
	using Attributes;
	using Events;
	[Serializable]
	public class Transition{
		public TransitionType type;
		public AttributeFloat time = 1;
		public AttributeFloat speed = 1;
		[Advanced] public AnimationCurve acceleration = AnimationCurve.EaseInOut(0,0,1,1);
		[Advanced] public AnimationCurve deceleration = AnimationCurve.EaseInOut(0,1,1,0);
		[Advanced][Internal] public AttributeFloat delta = 0;
		[NonSerialized] public TransitionState state;
		protected string path;
		protected Component parent;
		protected float startValue;
		protected float startTime;
		protected float endTime;
		protected float currentDistance;
		protected float totalDistance;
		protected float previousGoal;
		protected bool finished;
		public void Setup(string path,Component parent){
			this.path = path;
			this.parent = parent;
			this.time.Setup(path+"/Transition Time",parent);
			this.speed.Setup(path+"/Transition Speed",parent);
			this.delta.Setup(path+"/Transition Delta",parent);
			Event.Register(path+"/Start",parent);
			Event.Register(path+"/End",parent);
		}
		public int Step(int current,int goal){
			return (int)this.Step((float)current,(float)goal);
		}
		public float Step(float current,float goal){
			float output = 0;
			float speed = this.speed.Get();
			float remainingDistance = current.Distance(goal);
			if(speed <= 0){
				current = goal;
				output = goal;
			}
			if(current == goal){
				if(!this.finished){this.parent.CallEvent(this.path+"/End");}
				this.finished = true;
				this.delta.Set(0);
				this.state = TransitionState.Idle;
				return output;
			}
			if(this.state == TransitionState.Idle){
				this.finished = false;
				this.totalDistance = remainingDistance;
				this.startTime = Time.time + this.time;
				this.parent.CallEvent(this.path+"/Start");
				this.state = this.time <= 0 ? TransitionState.Travel : TransitionState.Acceleration;
				this.startValue = current;
				this.previousGoal = goal;
				speed = 0;
			}
			else if(this.state == TransitionState.Acceleration){
				bool halfTraveled = remainingDistance <= this.totalDistance * 0.5f;
				float transitionIn = (this.time - (this.startTime - Time.time)) / this.time;
				speed *= this.acceleration.Evaluate(transitionIn);
				if(transitionIn >= 1 || halfTraveled){
					this.currentDistance = this.startValue.Distance(current);
					this.state = TransitionState.Travel;
					if(halfTraveled){
						this.state = TransitionState.Deceleration;
						this.endTime = Time.time + (transitionIn*this.time);
					}
				}
			}
			else if(this.state == TransitionState.Travel){
				if(this.time > 0 && remainingDistance <= this.currentDistance){
					float percentRemaining = remainingDistance / this.currentDistance;
					this.state = TransitionState.Deceleration;
					this.endTime = Time.time + (this.time * percentRemaining);
				}
			}
			else if(this.state == TransitionState.Deceleration){
				float transitionOut = (this.time - (this.endTime - Time.time)) / this.time;
				speed *= this.deceleration.Evaluate(transitionOut);
			}
			if(this.previousGoal != goal){
				if(this.state != TransitionState.Travel){this.state = TransitionState.Idle;}
				this.previousGoal = goal;
			}
			speed *= this.GetTimeOffset();
			this.delta.Set(current.MoveTowards(goal,speed) - current);
			output = current + this.delta.Get();
			return output;
		}
		public float GetTimeOffset(){
			if(this.parent is StateMonoBehaviour){
				return this.parent.As<StateMonoBehaviour>().GetTimeOffset();
			}
			return Time.deltaTime;
		}
	}
	public enum TransitionType{Time,Speed};
	public enum TransitionState{Idle,Acceleration,Travel,Deceleration};
}