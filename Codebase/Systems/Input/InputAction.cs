using System;
using UnityEngine;
namespace Zios.Inputs{
	using Actions.TransitionComponents;
	[Serializable]
	public class InputAction{
		public string name;
		public Sprite helpImage;
		public Transition transition;
	}
}