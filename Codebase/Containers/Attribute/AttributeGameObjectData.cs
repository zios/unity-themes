using UnityEngine;
using Zios;
namespace Zios{
    [AddComponentMenu("")]
    public class AttributeGameObjectData : AttributeData<GameObject,AttributeGameObject,AttributeGameObjectData>{
	    public override GameObject HandleSpecial(){
		    GameObject value = this.value;
		    string special = AttributeGameObject.specialList[this.special];
		    if(special == "Parent"){return value.GetParent();}
		    return value;
	    }
    }
}