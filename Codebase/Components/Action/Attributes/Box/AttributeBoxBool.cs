using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Attribute/Box/Box Bool")]
public class AttributeBoxBool : AttributeBox<AttributeBool>{
	public override void Store(){
		PlayerPrefs.SetInt(this.value.info.path,this.value?1:0);
	}
	public override void Load(){
		bool value = PlayerPrefs.GetInt(this.value.info.path) == 1;
		this.value.Set(value);
	}
}