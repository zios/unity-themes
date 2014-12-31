using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Attribute/Box/Box String")]
public class AttributeBoxString : AttributeBox<AttributeString>{
	public override void Store(){
		PlayerPrefs.SetString(this.value.info.path,this.value);
	}
	public override void Load(){
		string value = PlayerPrefs.GetString(this.value.info.path);
		this.value.Set(value);
	}
}