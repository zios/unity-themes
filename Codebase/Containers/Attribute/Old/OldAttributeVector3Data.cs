using UnityEngine;
using Zios;
namespace Zios{
	[AddComponentMenu("")]
	public class OldAttributeVector3Data : OldAttributeData{
		public Vector3 value;
		public override AttributeData Convert(){
			var data = new AttributeVector3Data();
			data.rawValue = this.value.ToString();
			data.rawType = typeof(AttributeVector3Data).FullName;
			return data;
		}
	}
}