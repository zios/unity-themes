using Zios;
using UnityEngine;
namespace Zios{
    public enum ForceType{Absolute,Relative}
    [AddComponentMenu("Zios/Component/Action/Move/Add Force")]
    public class AddForce : ActionLink{
	    public ForceType type;
	    public AttributeVector3 amount = Vector3.zero;
	    public AttributeGameObject target = new AttributeGameObject();
	    public override void Awake(){
		    base.Awake();
		    this.target.Setup("Target",this);
		    this.amount.Setup("Amount",this);
		    this.AddDependent<ColliderController>(this.target);
		    this.AddDependent<Force>(this.target);
	    }
	    public override void Use(){
		    base.Use();
		    Vector3 amount = this.amount;
		    if(this.type == ForceType.Relative){
			    amount = this.target.Get().transform.right * this.amount.x;
			    amount += this.target.Get().transform.up * this.amount.y;
			    amount += this.target.Get().transform.forward * this.amount.z;
		    }
		    this.target.Get().Call("Add Force",amount);
	    }
    }
}