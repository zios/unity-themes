using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Clamp (Vector2)")]
public class ClampVector2 : ActionPart{
	public EventManageTarget target = new EventManageTarget();
	public Vector2 minimum;
	public Vector2 maximum;
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
		Vector2 value = (Vector2)this.target.Get();
		bool xExists = this.minimum.x != 0 && this.maximum.x != 0;
		bool yExists = this.minimum.y != 0 && this.maximum.y != 0;
		if(xExists && value.x < this.minimum.x){value.x = this.minimum.x;}
		if(yExists && value.y < this.minimum.y){value.y = this.minimum.y;}
		if(xExists && value.x > this.maximum.x){value.x = this.maximum.x;}
		if(yExists && value.y > this.maximum.y){value.y = this.maximum.y;}
		this.target.Set(value);
		base.Use();
	}
}
