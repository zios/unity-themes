using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Event/Event Listen")]
	public class EventListen : StateBehaviour{
		public EventTarget target = new EventTarget();
		private bool eventActive;
		public override void Awake(){
			base.Awake();
			this.DefaultRate("LateUpdate");
			this.target.Setup("Event",this);
			this.target.Listen(this.Listen);
			this.target.mode = EventMode.Callers;
		}
		public override void Use(){
			if(this.eventActive){
				base.Use();
				this.eventActive = false;
				return;
			}
			base.End();
		}
		public void Listen(){this.eventActive = true;}
	}
}