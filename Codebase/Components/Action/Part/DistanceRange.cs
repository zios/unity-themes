using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Distance Range")]
public class DistanceRange : ActionPart{
	public Target source = new Target();
	public Target target = new Target();
	public AttributeFloat minimumDistance = Mathf.Infinity;
	public AttributeFloat maximumDistance = 0;
	public override void OnValidate(){
		base.OnValidate();
		this.DefaultRate("LateUpdate");
		this.source.Setup("Source",this);
		this.target.Setup("Target",this);
		this.minimumDistance.Setup("MinimumDistance",this);
		this.maximumDistance.Setup("MaximumDistance",this);
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform target = this.target.Get().transform;
		float distance = Vector3.Distance(source.position,target.position);
		if(distance.Between(this.minimumDistance,this.maximumDistance)){
			base.Use();
		}
		else{
			base.End();
		}
	}
}
