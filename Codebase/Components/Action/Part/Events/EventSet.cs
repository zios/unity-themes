using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Event Set")]
public class EventSet : ActionPart{
	public EventSetTarget target = new EventSetTarget();
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
	}
	public override void Use(){
		this.target.Set();
		base.Use();
	}
}
