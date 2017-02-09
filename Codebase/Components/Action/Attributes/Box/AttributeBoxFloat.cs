using UnityEngine;
namespace Zios.Attributes{
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Float")]
	public class AttributeBoxFloat : AttributeBox<AttributeFloat>{
		public override void Store(){
			Utility.SetPlayerPref<float>(this.value.info.fullPath,this.value);
		}
		public override void Load(){
			float value = Utility.GetPlayerPref<float>(this.value.info.fullPath);
			this.value.Set(value);
		}
	}
}