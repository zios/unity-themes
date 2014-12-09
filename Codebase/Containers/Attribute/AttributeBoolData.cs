using UnityEngine;
using Zios;
[AddComponentMenu("")]
public class AttributeBoolData : AttributeData<bool,AttributeBool,AttributeBoolData,SpecialBool>{
	public override bool HandleSpecial(){
		bool value = this.value;
		if(this.attribute.mode == AttributeMode.Linked){return value;}
		else if(this.special == SpecialBool.Flip){return !value;}
		return value;
	}
}