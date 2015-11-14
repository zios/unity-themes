using UnityEngine;
using Zios;
namespace Zios{
	[AddComponentMenu("")]
	public class OldAttributeGameObjectData : OldAttributeData{
		public GameObject value;
		public override AttributeData Convert(){
			var data = new AttributeGameObjectData();
			data.value = this.value;
			data.rawValue = this.value.GetPath();
			data.rawType = typeof(AttributeGameObjectData).FullName;
			return data;
		}
	}
}