using System;
using UnityEngine;
namespace Zios.Inputs{
	using Actions.TransitionComponents;
	[Serializable]
	public class InputAction{
		public string name;
		public Transition acceleration = new Transition();
		public Transition deceleration = new Transition();
		public Sprite suggestion;
	}
}