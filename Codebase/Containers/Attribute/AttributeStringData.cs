using UnityEngine;
using Zios;
public class AttributeStringData : AttributeData<string,AttributeString,AttributeStringData,OperatorString,SpecialString>{
	public int characterLimit;
	public string[] allowed = new string[0];
	public string[] disallowed = new string[0];
	public override string HandleSpecial(){
		string value = this.value;
		if(this.attribute.mode == AttributeMode.Linked){return value;}
		else if(this.special == SpecialString.Lower){return value.ToLower();}
		else if(this.special == SpecialString.Upper){return value.ToUpper();}
		else if(this.special == SpecialString.Capitalize){return value.Capitalize();}
		return value;
	}
}