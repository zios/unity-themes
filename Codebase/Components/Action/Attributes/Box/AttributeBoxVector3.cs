using UnityEngine;
namespace Zios.Attributes{
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Vector3")]
	public class AttributeBoxVector3 : AttributeBox<AttributeVector3>{
		public override void Store(){
			string path = this.value.info.fullPath;
			Utility.SetPlayerPref<float>(path+"x",this.value.x);
			Utility.SetPlayerPref<float>(path+"y",this.value.y);
			Utility.SetPlayerPref<float>(path+"z",this.value.z);
		}
		public override void Load(){
			string path = this.value.info.fullPath;
			this.value.x = Utility.GetPlayerPref<float>(path+"x");
			this.value.y = Utility.GetPlayerPref<float>(path+"y");
			this.value.z = Utility.GetPlayerPref<float>(path+"z");
		}
	}
}