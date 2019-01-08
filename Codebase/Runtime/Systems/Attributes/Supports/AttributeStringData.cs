using UnityEngine;
namespace Zios.Attributes.Supports{
	using Zios.Extensions;
	[AddComponentMenu("")]
	public class AttributeStringData : AttributeData<string,AttributeString,AttributeStringData>{
		public int characterLimit;
		public string[] allowed = new string[0];
		public string[] disallowed = new string[0];
		public override string HandleSpecial(){
			string value = this.value;
			string special = AttributeString.specialList[this.special];
			if(this.attribute.mode == AttributeMode.Linked){return value;}
			else if(special == "Lower"){return value.ToLower();}
			else if(special == "Upper"){return value.ToUpper();}
			else if(special == "Capitalize"){return value.ToCapitalCase();}
			return value;
		}
	}
}