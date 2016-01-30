using UnityEngine;
namespace Zios.Attributes{
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Int")]
	public class AttributeBoxInt : AttributeBox<AttributeInt>{
		public override void Store(){
			PlayerPrefs.SetInt(this.value.info.fullPath,this.value);
		}
		public override void Load(){
			int value = PlayerPrefs.GetInt(this.value.info.fullPath);
			this.value.Set(value);
		}
	}
}