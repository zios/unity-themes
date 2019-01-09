using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.State;
	using Zios.SystemAttributes;
	using Zios.Unity.Components.ColliderController;
	using Zios.Unity.Extensions;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	//asm Zios.Unity.Shortcuts;
	public enum MoveType{Absolute,Relative}
	[AddComponentMenu("Zios/Component/Action/Motion/Add Move")]
	public class AddMove : StateBehaviour{
		public MoveType type;
		[Advanced] public string eventName;
		public AttributeGameObject target = new AttributeGameObject();
		public AttributeVector3 amount = Vector3.zero;
		public override void Awake(){
			this.eventName = this.eventName.SetDefault("Add Move");
			base.Awake();
			this.target.Setup("Target",this);
			this.amount.Setup("Amount",this);
			this.AddDependent<ColliderController>(this.target);
		}
		public override void Use(){
			base.Use();
			foreach(GameObject target in this.target){
				Vector3 amount = this.amount;
				if(this.type == MoveType.Relative){
					amount = target.transform.Localize(amount);
				}
				target.CallEvent(this.eventName,amount);
			}
		}
	}
}