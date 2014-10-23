using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Distance Range")]
public class DistanceRange : ActionPart{
	public Target source = new Target();
	public Target target = new Target();
	public float minimumDistance = Mathf.Infinity;
	public float maximumDistance;
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.source.Update(this);
		this.target.Update(this);
	}
	public void Start(){
		this.source.Setup(this,"Source");
		this.target.Setup(this);
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
