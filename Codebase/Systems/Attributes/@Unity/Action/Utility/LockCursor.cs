using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/General/Lock Cursor")]
	public class LockCursor : StateBehaviour{
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