using Zios;
using System;
using UnityEngine;
namespace Zios{
    public enum OffsetType{Relative,Absolute}
    [AddComponentMenu("Zios/Component/Action/Move/Follow Target")]
    public class FollowTarget : StateMonoBehaviour{
	    public AttributeGameObject source = new AttributeGameObject();
	    public AttributeGameObject target = new AttributeGameObject();
	    public LerpVector3 position = new LerpVector3();
	    public OffsetType offsetType;
	    public AttributeVector3 offset = Vector3.zero;
	    public AttributeVector3 orbit = Vector3.zero;
	    public override void Awake(){
		    base.Awake();
		    this.source.Setup("Source",this);
		    this.target.Setup("Target",this);
		    this.position.Setup("Follow",this);
		    this.offset.Setup("Follow Offset",this);
		    this.orbit.Setup("Follow Orbit",this);
			this.position.isAngle.Set(false);
	    }
	    public Vector3 AdjustVector(Vector3 value){
		    Vector3 adjusted = value;
		    if(this.offsetType == OffsetType.Relative){
			    Transform target = this.target.Get().transform;
			    adjusted = target.right * value.x;
			    adjusted += target.up * value.y;
			    adjusted += target.forward * value.z;
		    }
		    return adjusted;
	    }
	    public override void Use(){
		    Transform target = this.target.Get().transform;
		    Vector3 offset = this.AdjustVector(this.offset);
		    Vector3 orbit = this.orbit.Get().ScaleBy(new Vector3(1,-1,1));
            Vector3 end = (orbit.ToRotation() * offset) + target.position;
			foreach(GameObject source in this.source){
				source.transform.position = this.position.Step(source.transform.position,end);
			}
		    base.Use();
	    }
    }
}
