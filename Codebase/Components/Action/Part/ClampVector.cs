using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Clamp (Vector)")]
public class ClampVector : ActionPart{
	public StateMonoBehaviour component;
	public string attribute;
	public Vector2 minimum;
	public Vector2 maximum;
	private Accessor accessor;
	public void Start(){
		this.accessor = new Accessor(this.component,this.attribute);
	}
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		if(accessor == null){
			Debug.LogWarning("ClampVector : Accessor not found.");
			return;
		}
		Vector2 value = (Vector2)this.accessor.Get();
		bool xExists = this.minimum.x != 0 && this.maximum.x != 0;
		bool yExists = this.minimum.y != 0 && this.maximum.y != 0;
		if(xExists && value.x < this.minimum.x){value.x = this.minimum.x;}
		if(yExists && value.y < this.minimum.y){value.y = this.minimum.y;}
		if(xExists && value.x > this.maximum.x){value.x = this.maximum.x;}
		if(yExists && value.y > this.maximum.y){value.y = this.maximum.y;}
		this.accessor.Set(value);
		base.Use();
	}
}