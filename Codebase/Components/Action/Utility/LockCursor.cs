using UnityEngine;
namespace Zios.Actions.UtilityComponents{
	[AddComponentMenu("Zios/Component/Action/General/Lock Cursor")]
	public class LockCursor : StateMonoBehaviour{
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
}