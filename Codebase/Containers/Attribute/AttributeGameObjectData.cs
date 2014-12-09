using UnityEngine;
using Zios;
[AddComponentMenu("")]
public class AttributeGameObjectData : AttributeData<GameObject,AttributeGameObject,AttributeGameObjectData,SpecialGameObject>{
	public override GameObject HandleSpecial(){
		GameObject value = this.value;
		if(this.special == SpecialGameObject.Parent){return value.GetParent();}
		return value;
	}
}