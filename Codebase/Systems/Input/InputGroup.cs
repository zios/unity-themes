using System;
using System.Collections.Generic;
namespace Zios.Inputs{
	[Serializable]
	public class InputGroup{
		public string name;
		public List<InputAction> actions = new List<InputAction>();
	}
}