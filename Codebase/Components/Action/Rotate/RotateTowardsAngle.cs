using Zios;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Rotate/Towards/Rotate Towards Angle")]
    public class RotateTowardsAngle : ActionLink{
	    public AttributeGameObject source = new AttributeGameObject();
	    public AttributeVector3 goal = Vector3.zero;
	    public LerpVector3 rotation = new LerpVector3();
	    public override void Awake(){
		    base.Awake();
		    this.source.Setup("Source",this);
		    this.goal.Setup("Goal",this);
		    this.rotation.Setup("Rotation Angle",this);
		    this.rotation.isAngle.Set(true);
	    }
	    public override void Use(){
		    Transform transform = this.source.Get().transform;
		    Vector3 current = transform.localEulerAngles;
		    transform.localEulerAngles = this.rotation.Step(current,this.goal);
		    base.Use();
	    }
    }
}