using UnityEngine;
using Zios;
namespace Zios{
	[AddComponentMenu("")]
	public class OldAttributeIntData : OldAttributeData{
		public int value;
		public override AttributeData Convert(){
			var data = new AttributeIntData();
			data.value = this.value;
			data.rawValue = this.value.ToString();
			data.rawType = typeof(AttributeIntData).FullName;
			return data;
		}
	}
}