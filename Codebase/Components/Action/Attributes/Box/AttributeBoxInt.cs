using Zios;
using UnityEngine;
namespace Zios{
[AddComponentMenu("Zios/Component/Attribute/Box/Box Int")]
	public class AttributeBoxInt : AttributeBox<AttributeInt>{
		public override void Store(){
			PlayerPrefs.SetInt(this.value.info.path,this.value);
		}
		public override void Load(){
			int value = PlayerPrefs.GetInt(this.value.info.path);
			this.value.Set(value);
		}
	}
}