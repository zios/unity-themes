using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Events;
	using Zios.State;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	//asm Zios.Unity.Shortcuts;
	[AddComponentMenu("Zios/Component/Action/Event/Event Call")]
	public class EventCall : StateBehaviour{
		public EventTarget target = new EventTarget();
		public override void Awake(){
			base.Awake();
			this.target.Setup("Event",this);
			this.target.mode = EventMode.Listeners;
			Events.Add("On Validate",this.Register,this);
		}
		public void Register(){
			Debug.Log(this.target.name + "!");
			Events.Register(this.target.name,this);
		}
		public override void Use(){
			this.target.Call();
			base.Use();
		}
	}
}