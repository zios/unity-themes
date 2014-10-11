using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Look Trigger (Clamp)")]
public class LookClampTrigger : ActionPart{
	public LookType type;
	public Target source = new Target();
	public Target target = new Target();
	public ClerpVector3 lerp = new ClerpVector3();
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.source.AddSpecial("[Owner]",this.action.owner);
		this.source.AddSpecial("[Action]",this.action.gameObject);
		this.target.AddSpecial("[Owner]",this.action.owner);
		this.target.AddSpecial("[Action]",this.action.gameObject);
		this.source.DefaultSearch("[Owner]");
		this.target.DefaultSearch("[Owner]");
		this.lerp.isAngle = true;
	}
	public override void Use(){
		Transform source = this.source.Get().transform;
		Transform target = this.target.Get().transform;
		Vector3 current = source.localEulerAngles;
		Vector3 end = Vector3.zero;
		if(this.type == LookType.LookAt){
			source.LookAt(target);
			end = source.localEulerAngles;
			source.localEulerAngles = current;
		}
		if(this.type == LookType.LookWith){
			end = target.localEulerAngles;
		}
		source.localEulerAngles = this.lerp.Step(current,end);
		base.Use();
	}
	public override void End(){
		this.lerp.Reset();
		base.End();
	}
}