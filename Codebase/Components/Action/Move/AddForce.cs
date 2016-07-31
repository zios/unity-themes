using UnityEngine;
namespace Zios.Actions.MoveComponents{
	using Motion;
	[AddComponentMenu("Zios/Component/Action/Motion/Add Force")]
	public class AddForce : AddMove{
		public override void Awake(){
			this.eventName = this.eventName.SetDefault("Add Force");
			base.Awake();
			this.AddDependent<Force>(this.target);
		}
	}
}