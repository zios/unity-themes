using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Unity.Pref;
	[AddComponentMenu("Zios/Component/Attribute/Box/Box String")]
	public class AttributeBoxString : AttributeBox<AttributeString>{
		public override void Store(){
			PlayerPref.Set<string>(this.value.info.fullPath,this.value);
		}
		public override void Load(){
			string value = PlayerPref.Get<string>(this.value.info.fullPath);
			this.value.Set(value);
		}
	}
}