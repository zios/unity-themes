using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Lock Cursor")]
public class LockCursor : ActionLink{
	public override void Use(){
		base.Use();
		Screen.lockCursor = true;
	}
	public override void End(){
		base.End();
		Screen.lockCursor = false;
	}
}
