using UnityEngine;
namespace Zios.Attributes{
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Int")]
	public class AttributeBoxInt : AttributeBox<AttributeInt>{
		public override void Store(){
			Utility.SetPlayerPref<int>(this.value.info.fullPath,this.value);
		}
		public override void Load(){
			int value = Utility.GetPlayerPref<int>(this.value.info.fullPath);
			this.value.Set(value);
		}
	}
}