using UnityEngine;
using Zios;
public class AttributeGameObjectData : AttributeData<GameObject,AttributeGameObject,AttributeGameObjectData,OperatorGameObject,SpecialGameObject>{
	public override GameObject HandleSpecial(){
		GameObject value = this.value;
		if(this.special == SpecialGameObject.Parent){return value.GetParent();}
		return value;
	}
}