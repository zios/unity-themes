using System;
using UnityEngine;
namespace Zios.Containers.Math{
	using Events;
	[Serializable]
	public class LerpQuaternion : LerpTransition{
		public float endProximity;
		private Quaternion? lastStart;
		private Quaternion? lastEnd;
		public override void Setup(string path,Component parent){
			base.Setup(path,parent);
			Event.Register(this.path+"/Transition/On End",this.parent.gameObject);
			Event.Register(this.path+"/Transition/On Start",this.parent.gameObject);
		}
		public virtual Quaternion Step(Quaternion current){
			return this.Step(current,current);
		}
		public virtual Quaternion Step(Quaternion start,Quaternion end){
			float distance = (Quaternion.Inverse(start)*end).eulerAngles.magnitude;
			if(distance <= this.endProximity){
				if(this.active){
					this.parent.gameObject.CallEvent(this.path+"/Transition/On End");
					this.active = false;
				}
				return start;
			}
			if(this.isResetOnChange){
				if(this.lastEnd != end){
					this.Reset();
					this.active = false;
				}
			}
			if(!this.active){
				this.transition.Reset();
				this.parent.gameObject.CallEvent(this.path+"/Transition/On Start");
				this.lastStart = start;
				this.lastEnd = end;
				this.active = true;
			}
			float percent = this.transition.Tick();
			Quaternion current = start;
			if(this.speed != 0){
				float speed = this.speed * percent;
				speed *= this.fixedTime ? Time.fixedDeltaTime : Time.deltaTime;
				current = Quaternion.RotateTowards(start,end,speed);
			}
			else{
				current = Quaternion.Slerp((Quaternion)this.lastStart,end,percent);
			}
			return current;
		}
	}
}