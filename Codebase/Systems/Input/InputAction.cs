using System;
using UnityEngine;
namespace Zios.Inputs{
	using Actions.TransitionComponents;
	[Serializable]
	public class InputAction{
		public string name;
		public string path;
		[EnumMask] public InputActionOptions options;
		public Sprite helpImage;
		public Transition transition;
		public InputAction(){}
		public InputAction(InputAction action){
			this.name = action.name;
			this.options = action.options;
			this.transition = new Transition(action.transition);
		}
		public void Setup(string path,Component parent){
			this.path = path + "/" + this.name;
			if(this.transition.IsNull() || this.transition.acceleration.keys.Length < 1){
				this.transition = new Transition();
			}
			this.transition.Setup(this.path,parent);
		}
		public InputAction Copy(){return new InputAction(this);}
	}
	public enum InputActionOptions{Unclamped=1,AxisMode=2}
}