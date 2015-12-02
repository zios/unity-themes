using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Move/Add Force")]
	public class AddForce : AddMove{
		public override void Awake(){
			this.eventName = this.eventName.SetDefault("Add Force");
			base.Awake();
			this.AddDependent<Force>(this.target);
		}
	}
}