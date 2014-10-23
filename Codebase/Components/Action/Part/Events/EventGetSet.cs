using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Get Set")]
public class EventGetSet : ActionPart{
	public EventGetTarget source = new EventGetTarget();
	public EventSetTarget target = new EventSetTarget();
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.source.Update(this);
		this.target.Update(this);
	}
	public void Start(){
		this.source.Setup(this);
		this.target.Setup(this);
	}
	public override void Use(){
		object value = this.source.Get();
		this.target.Set(value);
		base.Use();
	}
}
