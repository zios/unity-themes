using Zios;
using UnityEngine;
using System;
namespace Zios{
	public enum UpdateRate{Default,FixedUpdate,Update,LateUpdate,None};
	[Serializable][AddComponentMenu("")]
	public class ManagedMonoBehaviour : DataMonoBehaviour{
		[Advanced] public UpdateRate rate = UpdateRate.Default;
		public float GetTimeOffset(){
			if(this.rate == UpdateRate.FixedUpdate || this.rate == UpdateRate.Default){
				return Time.fixedDeltaTime;
			}
			return Time.deltaTime;
		}
		public void DefaultRate(string rate){
			if(this.rate == UpdateRate.Default){
				if(rate == "FixedUpdate"){this.rate = UpdateRate.FixedUpdate;}
				if(rate == "LateUpdate"){this.rate = UpdateRate.LateUpdate;}
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