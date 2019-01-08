using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.State;
	using Zios.SystemAttributes;
	using Zios.Unity.Time;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	public enum TimerType{After,During}
	[AddComponentMenu("Zios/Component/Action/General/Timer State")]
	public class TimerState : StateBehaviour{
		public TimerType type;
		public AttributeFloat seconds = 0;
		[Advanced][ReadOnly] public AttributeBool isStarted = false;
		[Advanced][ReadOnly] public AttributeBool isComplete = false;
		private float endTime;
		public override void Awake(){
			base.Awake();
			this.seconds.Setup("Seconds",this);
			this.isStarted.Setup("Is Started",this);
			this.isComplete.Setup("Is Complete",this);
		}
		public override void Step(){
			base.Step();
			if(!this.usable && this.isStarted){
				this.End();
			}
		}
		public override void Use(){
			if(this.isComplete){return;}
			if(!this.isStarted){
				this.endTime = Time.Get() + this.seconds.Get();
				this.isStarted.Set(true);
			}
			bool hasElapsed = Time.Get() > this.endTime;
			if(this.type == TimerType.After && hasElapsed){
				this.isComplete.Set(true);
				base.Use();
			}
			else if(this.type == TimerType.During){
				base.Use();
				if(hasElapsed){
					this.isComplete.Set(true);
					base.End();
				}
			}
		}
		public override void End(){
			this.isComplete.Set(false);
			this.isStarted.Set(false);
			base.End();
		}
	}
}