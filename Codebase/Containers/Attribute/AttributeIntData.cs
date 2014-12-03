using UnityEngine;
using Zios;
public class AttributeIntData : AttributeData<int,AttributeInt,AttributeIntData,OperatorNumeral,SpecialNumeral>{
	public override int HandleSpecial(){
		int value = this.value;
		if(this.attribute.mode == AttributeMode.Linked){return value;}
		else if(this.special == SpecialNumeral.Flip){return value * -1;}
		else if(this.special == SpecialNumeral.Abs){return Mathf.Abs(value);}
		else if(this.special == SpecialNumeral.Sign){return (int)Mathf.Sign(value);}
		else if(this.special == SpecialNumeral.Floor){return (int)Mathf.Floor(value);}
		else if(this.special == SpecialNumeral.Ceil){return (int)Mathf.Ceil(value);}
		else if(this.special == SpecialNumeral.Cos){return (int)Mathf.Cos(value);}
		else if(this.special == SpecialNumeral.Sin){return (int)Mathf.Sin(value);}
		else if(this.special == SpecialNumeral.Tan){return (int)Mathf.Tan(value);}
		else if(this.special == SpecialNumeral.ATan){return (int)Mathf.Atan(value);}
		else if(this.special == SpecialNumeral.Sqrt){return (int)Mathf.Sqrt(value);}
		return value;
	}
}