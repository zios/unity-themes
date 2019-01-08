using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Extensions;
	using Zios.Unity.Components.Force;
	[AddComponentMenu("Zios/Component/Action/Motion/Add Force")]
	public class AddForce : AddMove{
		public override void Awake(){
			this.eventName = this.eventName.SetDefault("Add Force");
			base.Awake();
			this.AddDependent<Force>(this.target);
		}
	}
}