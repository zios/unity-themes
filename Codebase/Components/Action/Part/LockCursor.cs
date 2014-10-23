using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Lock Cursor")]
public class LockCursor : ActionPart{
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
