using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Lock Cursor")]
public class LockCursor : ActionLink{
	public override void Use(){
		base.Use();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	public override void End(){
		base.End();
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}
