using Zios;
using UnityEngine;
    namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Rotate/Towards/Rotate Towards Direction")]
    public class RotateTowardsDirection : ActionLink{
	    public AttributeGameObject source = new AttributeGameObject();
	    public AttributeVector3 goal = Vector3.zero;
	    public ListBool lerpAxes = new ListBool(){true,true,true};
	    public LerpQuaternion rotation = new LerpQuaternion();
	    public override void Awake(){
		    base.Awake();
		    this.source.Setup("Source",this);
		    this.goal.Setup("Goal Direction",this);
		    this.rotation.Setup("Rotate Direction",this);
		    this.rotation.isResetOnChange.Set(false);
		    this.rotation.isResetOnChange.showInEditor = false;
		    this.rotation.isAngle.showInEditor = false;
	    }
	    public override void Use(){
		    GameObject source = this.source.Get();
		    Vector3 goal = this.goal.Get();
		    if(!goal.IsNull() && !source.IsNull()){
			    Vector3 angle = source.transform.eulerAngles;
			    Quaternion current = source.transform.rotation;
			    if(goal != Vector3.zero){
				    source.transform.rotation = Quaternion.LookRotation(goal);
			    }
			    source.transform.rotation = this.rotation.Step(current,source.transform.rotation);
			    if(this.lerpAxes[1]){angle.x = source.transform.eulerAngles.x;}
			    if(this.lerpAxes[0]){angle.y = source.transform.eulerAngles.y;}
			    if(this.lerpAxes[2]){angle.z = source.transform.eulerAngles.z;}
			    source.transform.eulerAngles = angle;
			    base.Use();
		    }
	    }
    }
}