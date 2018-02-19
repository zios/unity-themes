using System;
using UnityEngine;
namespace Zios.Unity.Components.ManagedBehaviour{
	using Zios.SystemAttributes;
	using Zios.Unity.Components.DataBehaviour;
	using Zios.Unity.Time;
	public enum UpdateRate{Default,FixedUpdate,Update,LateUpdate,None};
	[Serializable][AddComponentMenu("")]
	public class ManagedBehaviour : DataBehaviour{
		[Advanced] public UpdateRate rate = UpdateRate.Default;
		public float GetTimeOffset(){
			if(this.rate == UpdateRate.FixedUpdate || this.rate == UpdateRate.Default){
				return Time.GetFixedDelta();
			}
			return Time.GetDelta();
		}
		public void DefaultRate(string rate){
			if(this.rate == UpdateRate.Default){
				if(rate == "FixedUpdate"){this.rate = UpdateRate.FixedUpdate;}
				if(rate == "LateUpdate"){this.rate = UpdateRate.LateUpdate;}
				if(rate == "Update"){this.rate = UpdateRate.Update;}
			}
		}
		public virtual void FixedUpdate(){
			if(!Application.isPlaying){return;}
			if(this.rate == UpdateRate.FixedUpdate || this.rate == UpdateRate.Default){
				this.Step();
			}
		}
		public virtual void Update(){
			if(!Application.isPlaying){return;}
			if(this.rate == UpdateRate.Update){
				this.Step();
			}
		}
		public virtual void LateUpdate(){
			if(!Application.isPlaying){return;}
			if(this.rate == UpdateRate.LateUpdate){
				this.Step();
			}
		}
		public virtual void Step(){}
	}
}