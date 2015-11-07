using UnityEngine;
using Zios;
namespace Zios{
	[AddComponentMenu("")]
	public class OldAttributeFloatData : OldAttributeData{
		public float value;
		public override AttributeData Convert(){
			var data = new AttributeFloatData();
			data.rawValue = this.value.ToString();
			data.rawType = typeof(AttributeFloatData).FullName;
			return data;
		}
	}
}