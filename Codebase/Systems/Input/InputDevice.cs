using System;
namespace Zios.Inputs{
	[Serializable]
	public class InputDevice{
		public string name;
		public int id;
		public InputDevice(string name,int id=-1){
			this.name = name;
			this.id = id;
		}
	}
}