using UnityEngine;
using Zios;
public class AttributeVector3Data : AttributeData<Vector3,AttributeVector3,AttributeVector3Data,OperatorVector3,SpecialVector3>{
	public override Vector3 HandleSpecial(){
		Vector3 value = this.value;
		if(this.attribute.mode == AttributeMode.Linked){return value;}
		else if(this.special == SpecialVector3.Flip){return value * -1;}
		else if(this.special == SpecialVector3.Abs){return value.Abs();}
		else if(this.special == SpecialVector3.Sign){return value.Sign();}
		return value;
	}
}
