using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Lock Cursor Trigger")]
public class LockCursorTrigger : ActionPart{
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		base.Use();
		Screen.lockCursor = true;
	}
	public override void End(){
		base.End();
		Screen.lockCursor = false;
	}
}