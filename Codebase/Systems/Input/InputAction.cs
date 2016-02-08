using System;
namespace Zios.Inputs{
	using Containers.Math;
	[Serializable]
	public class InputAction{
		public string name;
		public Transition acceleration = new Transition();
		public Transition deceleration = new Transition();
		public GamepadKey recommendedGamepadKey;
	}
}