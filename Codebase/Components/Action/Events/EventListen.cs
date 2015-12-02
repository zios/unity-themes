using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Event/Event Listen")]
	public class EventListen : StateMonoBehaviour{
		public EventTarget target = new EventTarget();
		public override void Awake(){
			base.Awake();
			this.target.Setup("Event",this);
			this.target.SetupCatch(this.Catch);
			this.target.mode = EventMode.Callers;
		}
		public override void Use(){}
		public void Catch(){
			if(this.gameObject.activeSelf){
				base.Use();
			}
		}
	}
}