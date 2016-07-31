using UnityEngine;
namespace Zios.Actions.MoveComponents{
	using Attributes;
	using Events;
	using Motion;
	public enum MoveType{Absolute,Relative}
	[AddComponentMenu("Zios/Component/Action/Motion/Add Move")]
	public class AddMove : StateMonoBehaviour{
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