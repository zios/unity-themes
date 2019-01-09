using System;
using UnityObject = UnityEngine.Object;
namespace Zios.Inputs{
	using Zios.Supports.Transition;
	using Zios.Extensions;
	using Zios.Unity.SystemAttributes;
	[Serializable]
	public class InputAction{
		public string name;
		public string path;
		[EnumMask] public InputActionOptions options;
		public string helpImage;
		public Transition transition = new Transition();
		public InputAction(){}
		public InputAction(InputAction action){
			this.name = action.name;
			this.options = action.options;
			this.transition = new Transition(action.transition);
		}
		public void Setup(string path,UnityObject parent){
			this.path = path + "/" + this.name;
			if(this.transition.IsNull() || this.transition.acceleration.keys.Length < 1){
				this.transition = new Transition();
			}
			this.transition.Setup(this.path,parent);
		}
		public InputAction Copy(){return new InputAction(this);}
	}
	public enum InputActionOptions{Unclamped=1,AxisMode=2,AllowMouseMove=3}
}