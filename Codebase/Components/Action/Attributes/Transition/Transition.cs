using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Actions.TransitionComponents{
	using Attributes;
	using Event;
	public enum TransitionType{Time,Speed};
	public enum TransitionState{Idle,Acceleration,Travel,Deceleration};
	[Serializable]
	public class TransitionBase{
		//public TransitionType type;
		[Advanced] public AnimationCurve acceleration = AnimationCurve.EaseInOut(0,0,1,1);
		[Advanced] public AnimationCurve deceleration = AnimationCurve.EaseInOut(1,1,1,1);
		[NonSerialized] public TransitionState state;
		[HideInInspector] public UnityObject parent;
		[HideInInspector] public string path;
		protected float startValue;
		protected float startTime;
		protected float endTime;
		protected float currentDistance;
		protected float totalDistance;
		protected float previousGoal;
		protected bool finished;
		public TransitionBase(){}
		public TransitionBase(Transition transition){
			this.acceleration = new AnimationCurve().Deserialize(transition.acceleration.Serialize());
			this.deceleration = new AnimationCurve().Deserialize(transition.deceleration.Serialize());
		}
		public virtual float GetTime(){return 0;}
		public virtual float GetSpeed(){return 0;}
		public virtual float GetDelta(){return 0;}
		public virtual void SetDelta(float value){}
		public virtual void SetTime(float value){}
		public virtual void SetSpeed(float value){}
		public virtual void Setup(string path,UnityObject parent){
			this.path = path;
			this.parent = parent;
		}
		public int Step(int current,int goal){
			return (int)this.Step((float)current,(float)goal);
		}
		public float Step(float current,float goal){
			float output = 0;
			float speed = this.GetSpeed();
			float time = this.GetTime();
			if(time == 0 && speed == 0){
				current = goal;
				output = goal;
				return goal;
			}
			float remainingDistance = current.Distance(goal);
			if(remainingDistance <= 0){
				if(!this.finished){this.parent.CallEvent(this.path+"/End");}
				this.finished = true;
				this.SetDelta(0);
				this.state = TransitionState.Idle;
				return goal;
			}
			if(this.state == TransitionState.Idle){
				this.parent.CallEvent(this.path+"/Start");
				this.finished = false;
				this.totalDistance = remainingDistance;
				this.startTime = Time.time + time;
				this.state = time <= 0 ? TransitionState.Travel : TransitionState.Acceleration;
				this.startValue = current;
				this.previousGoal = goal;
				speed = 0;
			}
			else if(this.state == TransitionState.Acceleration){
				bool halfTraveled = remainingDistance <= this.totalDistance * 0.5f;
				float transitionIn = (time - (this.startTime - Time.time)) / time;
				speed *= this.acceleration.Evaluate(transitionIn);
				if(transitionIn >= 1 || halfTraveled){
					this.currentDistance = this.startValue.Distance(current);
					this.state = TransitionState.Travel;
					if(halfTraveled){
						this.state = TransitionState.Deceleration;
						this.endTime = Time.time + (transitionIn*time);
					}
				}
			}
			else if(this.state == TransitionState.Travel){
				if(time > 0 && remainingDistance <= this.currentDistance){
					float percentRemaining = remainingDistance / this.currentDistance;
					this.state = TransitionState.Deceleration;
					this.endTime = Time.time + (time * percentRemaining);
				}
			}
			else if(this.state == TransitionState.Deceleration){
				float transitionOut = (time - (this.endTime - Time.time)) / time;
				speed *= this.deceleration.Evaluate(transitionOut);
			}
			if(this.previousGoal != goal){
				if(this.state != TransitionState.Travel){this.state = TransitionState.Idle;}
				this.previousGoal = goal;
			}
			speed *= this.GetTimeOffset();
			this.SetDelta((current.MoveTowards(goal,speed) - current));
			output = current + this.GetDelta();
			return output;
		}
		public float GetTimeOffset(){
			if(this.parent is StateMonoBehaviour){
				return this.parent.As<StateMonoBehaviour>().GetTimeOffset();
			}
			return Time.deltaTime;
		}
	}
	[Serializable]
	public class Transition : TransitionBase{
		public float time = 0.5f;
		public float speed = 3;
		[HideInInspector] public float delta = 0;
		public Transition(){}
		public Transition(Transition transition) : base(transition){
			this.time = transition.time;
			this.speed = transition.speed;
		}
		public Transition(float time,float speed){
			this.time = time;
			this.speed = speed;
		}
		public override float GetTime(){return this.time;}
		public override float GetSpeed(){return this.speed;}
		public override float GetDelta(){return this.delta;}
		public override void SetDelta(float value){this.delta = value;}
		public override void SetTime(float value){this.time = value;}
		public override void SetSpeed(float value){this.speed = value;}
	}
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