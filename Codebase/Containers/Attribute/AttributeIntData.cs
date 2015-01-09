using UnityEngine;
using Zios;
[AddComponentMenu("")]
public class AttributeIntData : AttributeData<int,AttributeInt,AttributeIntData>{
	public override int HandleSpecial(){
		int value = this.value;
		string special = AttributeInt.specialList[this.special];
		if(this.attribute.mode == AttributeMode.Linked){return value;}
		else if(special == "Flip"){return value * -1;}
		else if(special == "Abs"){return Mathf.Abs(value);}
		else if(special == "Sign"){return (int)Mathf.Sign(value);}
		else if(special == "Floor"){return (int)Mathf.Floor(value);}
		else if(special == "Ceil"){return (int)Mathf.Ceil(value);}
		else if(special == "Cos"){return (int)Mathf.Cos(value);}
		else if(special == "Sin"){return (int)Mathf.Sin(value);}
		else if(special == "Tan"){return (int)Mathf.Tan(value);}
		else if(special == "ATan"){return (int)Mathf.Atan(value);}
		else if(special == "Sqrt"){return (int)Mathf.Sqrt(value);}
		return value;
	}
}