using Zios;
using UnityEngine;
namespace Zios{
    public enum MoveType{Absolute,Relative}
    [AddComponentMenu("Zios/Component/Action/Move/Add Move")]
    public class AddMove : ActionLink{
	    public MoveType type;
	    public AttributeGameObject target = new AttributeGameObject();
	    public AttributeVector3 amount = Vector3.zero;
		protected string eventName = "Add Move";
		protected string eventNameOnce = "Add Move Raw";
	    public override void Awake(){
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
				if(this.occurrence == ActionOccurrence.Once){
					target.CallEvent(this.eventNameOnce,amount*this.GetTimeOffset());
					continue;
				}
				target.CallEvent(this.eventName,amount);
			}
	    }
    }
}