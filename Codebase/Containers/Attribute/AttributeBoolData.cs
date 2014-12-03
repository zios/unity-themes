using UnityEngine;
using Zios;
public class AttributeBoolData : AttributeData<bool,AttributeBool,AttributeBoolData,OperatorBool,SpecialBool>{
	public override bool HandleSpecial(){
		bool value = this.value;
		if(this.attribute.mode == AttributeMode.Linked){return value;}
		else if(this.special == SpecialBool.Flip){return !value;}
		return value;
	}
}