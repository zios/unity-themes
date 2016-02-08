using System;
using System.Collections.Generic;
namespace Zios.Inputs{
	[Serializable]
	public class InputInstance{
		public string name;
		public InputProfile profile;
		public Dictionary<string,bool> active = new Dictionary<string,bool>();
		public Dictionary<string,float> intensity = new Dictionary<string,float>();
		public InputInstance(string name){this.name = name;}
	}
}