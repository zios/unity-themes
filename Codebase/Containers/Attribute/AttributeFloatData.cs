using UnityEngine;
using Zios;
[AddComponentMenu("")]
public class AttributeFloatData : AttributeData<float,AttributeFloat,AttributeFloatData,SpecialNumeral>{
	public override float HandleSpecial(){
		float value = this.value;
		if(this.attribute.mode == AttributeMode.Linked){return value;}
		else if(this.special == SpecialNumeral.Flip){return value * -1;}
		else if(this.special == SpecialNumeral.Abs){return Mathf.Abs(value);}
		else if(this.special == SpecialNumeral.Sign){return Mathf.Sign(value);}
		else if(this.special == SpecialNumeral.Floor){return Mathf.Floor(value);}
		else if(this.special == SpecialNumeral.Ceil){return Mathf.Ceil(value);}
		else if(this.special == SpecialNumeral.Cos){return Mathf.Cos(value);}
		else if(this.special == SpecialNumeral.Sin){return Mathf.Sin(value);}
		else if(this.special == SpecialNumeral.Tan){return Mathf.Tan(value);}
		else if(this.special == SpecialNumeral.ATan){return Mathf.Atan(value);}
		else if(this.special == SpecialNumeral.Sqrt){return Mathf.Sqrt(value);}
		return value;
	}
}