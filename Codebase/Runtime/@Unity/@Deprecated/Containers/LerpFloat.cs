using System;
namespace Zios.Unity.Deprecated{
	using Zios.Attributes.Deprecated;
	//asm Zios.Attributes.Supports;
	[Serializable]
	public class LerpFloat : LerpTransition{
		private float start;
		private float lastEnd;
		public float Step(float current){
			return this.Step(current,current);
		}
		public float Step(float start,float end){
			if(end != this.lastEnd && this.isResetOnChange){this.Reset();}
			if(!this.active){this.start = start;}
			this.lastEnd = end;
			this.CheckActive();
			return this.Lerp(this.start,end,this.transition.Tick());
		}
	}
}