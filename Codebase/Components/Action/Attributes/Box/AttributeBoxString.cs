using UnityEngine;
namespace Zios.Attributes{
	[AddComponentMenu("Zios/Component/Attribute/Box/Box String")]
	public class AttributeBoxString : AttributeBox<AttributeString>{
		public override void Store(){
			Utility.SetPlayerPref<string>(this.value.info.fullPath,this.value);
		}
		public override void Load(){
			string value = Utility.GetPlayerPref<string>(this.value.info.fullPath);
			this.value.Set(value);
		}
	}
}