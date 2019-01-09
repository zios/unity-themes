using System;
using UnityEngine;
namespace Zios.Attributes.Deprecated{
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.Reflection;
	using Zios.Unity.Time;
	[Serializable]
	public class Transition{
		public AttributeFloat duration = 0.5f;
		public AttributeFloat delayStart = 0;
		public AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
		[NonSerialized] public bool complete = true;
		[NonSerialized] public float endTime;
		[NonSerialized] public float startTime;
		public void Reset(){
			this.startTime = Time.Get() + this.delayStart;
			this.endTime = Time.Get() + this.duration + this.delayStart;
		}
		public virtual void Setup(string path,Component parent){
			path = (parent.GetAlias() + "/" + path).Trim("/");
			this.duration.Setup(path+"/Duration",parent);
			this.delayStart.Setup(path+"/Delay Start",parent);
			this.duration.locked = true;
			this.delayStart.locked = true;
		}
		public void End(){
			this.endTime = 0;
			this.complete = true;
		}
		public float Tick(){
			float startTime = this.endTime - this.duration;
			if(Time.Get() < this.startTime){return 0;}
			float elapsed = this.duration <= 0 ? 1 : (Time.Get()-startTime)/this.duration;
			this.complete = Time.Get() >= endTime;
			return this.curve.Evaluate(elapsed);
		}
	}
}