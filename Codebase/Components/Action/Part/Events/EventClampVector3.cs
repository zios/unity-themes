using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Clamp (Vector3)")]
public class ClampVector3 : ActionPart{
	public EventManageTarget target = new EventManageTarget();
	public Vector3 minimum;
	public Vector3 maximum;
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
	}
	public override void Use(){
		Vector3 value = (Vector3)this.target.Get();
		bool xExists = this.minimum.x != 0 && this.maximum.x != 0;
		bool yExists = this.minimum.y != 0 && this.maximum.y != 0;
		bool zExists = this.minimum.z != 0 && this.maximum.z != 0;
		if(xExists && value.x < this.minimum.x){value.x = this.minimum.x;}
		if(xExists && value.x > this.maximum.x){value.x = this.maximum.x;}
		if(yExists && value.y < this.minimum.y){value.y = this.minimum.y;}
		if(yExists && value.y > this.maximum.y){value.y = this.maximum.y;}
		if(zExists && value.z < this.minimum.z){value.z = this.minimum.z;}
		if(zExists && value.z > this.maximum.z){value.z = this.maximum.z;}
		this.target.Set(value);
		base.Use();
	}
}
