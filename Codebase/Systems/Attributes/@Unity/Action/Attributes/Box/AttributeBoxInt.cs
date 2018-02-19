using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Unity.Pref;
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Int")]
	public class AttributeBoxInt : AttributeBox<AttributeInt>{
		public override void Store(){
			PlayerPref.Set<int>(this.value.info.fullPath,this.value);
		}
		public override void Load(){
			int value = PlayerPref.Get<int>(this.value.info.fullPath);
			this.value.Set(value);
		}
	}
}