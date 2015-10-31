using Zios;
using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Float")]
	public class AttributeBoxFloat : AttributeBox<AttributeFloat>{
		public override void Store(){
			PlayerPrefs.SetFloat(this.value.info.path,this.value);
		}
		public override void Load(){
			float value = PlayerPrefs.GetFloat(this.value.info.path);
			this.value.Set(value);
		}
	}
}