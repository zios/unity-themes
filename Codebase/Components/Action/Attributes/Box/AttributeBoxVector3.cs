using Zios;
using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Attribute/Box/Box Vector3")]
	public class AttributeBoxVector3 : AttributeBox<AttributeVector3>{
		public override void Store(){
			string path = this.value.info.path;
			PlayerPrefs.SetFloat(path+"x",this.value.x);
			PlayerPrefs.SetFloat(path+"y",this.value.y);
			PlayerPrefs.SetFloat(path+"z",this.value.z);
		}
		public override void Load(){
			string path = this.value.info.path;
			this.value.x = PlayerPrefs.GetFloat(path+"x");
			this.value.y = PlayerPrefs.GetFloat(path+"y");
			this.value.z = PlayerPrefs.GetFloat(path+"z");
		}
	}
}