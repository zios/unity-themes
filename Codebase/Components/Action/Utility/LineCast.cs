using Zios;
using System;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Line Cast")]
    public class LineCast : ActionLink{
	    public Color rayColor = Color.blue;
	    public AttributeVector3 source = Vector3.zero;
	    public AttributeVector3 goal = Vector3.zero;
	    public LayerMask layers = -1;
	    [HideInInspector] public RaycastHit cast = new RaycastHit();
	    [HideInInspector] public AttributeVector3 hitPoint = Vector3.zero;
	    [HideInInspector] public AttributeVector3 hitNormal = Vector3.zero;
	    [HideInInspector] public AttributeFloat hitDistance = 0;
	    public override void Awake(){
		    base.Awake();
		    this.source.Setup("Source",this);
		    this.goal.Setup("Goal",this);
		    this.hitPoint.Setup("Hit Point",this);
		    this.hitNormal.Setup("Hit Normal",this);
		    this.hitDistance.Setup("Hit Distance",this);
	    }
	    public override void Use(){
		    bool state = Physics.Linecast(this.source,this.goal,out this.cast,this.layers.value);
		    this.Toggle(state);
		    this.hitPoint.Set(this.cast.point);
		    this.hitNormal.Set(this.cast.normal);
		    this.hitDistance.Set(this.cast.distance);
	    }
	    public void OnDrawGizmosSelected(){
			Gizmos.color = this.rayColor;
			Gizmos.DrawLine(this.source,this.goal);
	    }
    }
}