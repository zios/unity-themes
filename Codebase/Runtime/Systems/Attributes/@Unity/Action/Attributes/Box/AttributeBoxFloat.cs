using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Unity.Pref;
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Float")]
	public class AttributeBoxFloat : AttributeBox<AttributeFloat>{
		public override void Store(){
			PlayerPref.Set<float>(this.value.info.fullPath,this.value);
		}
		public override void Load(){
			float value = PlayerPref.Get<float>(this.value.info.fullPath);
			this.value.Set(value);
		}
	}
}