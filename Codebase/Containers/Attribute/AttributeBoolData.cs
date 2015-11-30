using UnityEngine;
using Zios;
namespace Zios{
	[AddComponentMenu("")]
	public class AttributeBoolData : AttributeData<bool,AttributeBool,AttributeBoolData>{
		public override bool HandleSpecial(){
			bool value = this.value;
			string special = AttributeBool.specialList[this.special];
			if(this.attribute.mode == AttributeMode.Linked){return value;}
			else if(special == "Flip"){return !value;}
			return value;
		}
	}
}