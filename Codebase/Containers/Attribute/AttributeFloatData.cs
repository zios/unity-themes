using UnityEngine;
using Zios;
[AddComponentMenu("")]
public class AttributeFloatData : AttributeData<float,AttributeFloat,AttributeFloatData>{
	public override float HandleSpecial(){
		float value = this.value;
		string special = AttributeFloat.specialList[this.special];
		if(this.attribute.mode == AttributeMode.Linked){return value;}
		else if(special == "Flip"){return value * -1;}
		else if(special == "Abs"){return Mathf.Abs(value);}
		else if(special == "Sign"){return Mathf.Sign(value);}
		else if(special == "Floor"){return Mathf.Floor(value);}
		else if(special == "Ceil"){return Mathf.Ceil(value);}
		else if(special == "Cos"){return Mathf.Cos(value);}
		else if(special == "Sin"){return Mathf.Sin(value);}
		else if(special == "Tan"){return Mathf.Tan(value);}
		else if(special == "ATan"){return Mathf.Atan(value);}
		else if(special == "Sqrt"){return Mathf.Sqrt(value);}
		return value;
	}
}