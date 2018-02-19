using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Unity.Pref;
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Bool")]
	public class AttributeBoxBool : AttributeBox<AttributeBool>{
		public override void Store(){
			PlayerPref.Set<int>(this.value.info.fullPath,this.value?1:0);
		}
		public override void Load(){
			bool value = PlayerPref.Get<int>(this.value.info.fullPath) == 1;
			this.value.Set(value);
		}
	}
}