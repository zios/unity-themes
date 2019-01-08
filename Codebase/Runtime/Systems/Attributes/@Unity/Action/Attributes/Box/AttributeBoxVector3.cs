using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Unity.Pref;
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Vector3")]
	public class AttributeBoxVector3 : AttributeBox<AttributeVector3>{
		public override void Store(){
			string path = this.value.info.fullPath;
			PlayerPref.Set<float>(path+"x",this.value.x);
			PlayerPref.Set<float>(path+"y",this.value.y);
			PlayerPref.Set<float>(path+"z",this.value.z);
		}
		public override void Load(){
			string path = this.value.info.fullPath;
			this.value.x = PlayerPref.Get<float>(path+"x");
			this.value.y = PlayerPref.Get<float>(path+"y");
			this.value.z = PlayerPref.Get<float>(path+"z");
		}
	}
}