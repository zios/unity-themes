using UnityEngine;
namespace Zios.Actions.EventComponents{
	using Events;
	[AddComponentMenu("Zios/Component/Action/Event/Event Call")]
	public class EventCall : StateMonoBehaviour{
		public EventTarget target = new EventTarget();
		public override void Awake(){
			base.Awake();
			this.target.Setup("Event",this);
			this.target.mode = EventMode.Listeners;
			Event.Add("On Validate",this.Register,this);
		}
		public void Register(){
			Debug.Log(this.target.name + "!");
			Event.Register(this.target.name,this);
		}
		public override void Use(){
			this.target.Call();
			base.Use();
		}
	}
}