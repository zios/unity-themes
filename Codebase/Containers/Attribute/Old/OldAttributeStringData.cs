using UnityEngine;
namespace Zios{
	[AddComponentMenu("")]
	public class OldAttributeStringData : OldAttributeData{
		public string value;
		public override AttributeData Convert(){
			var data = new AttributeStringData();
			data.rawValue = this.value.ToString();
			data.rawType = typeof(AttributeStringData).FullName;
			return data;
		}
	}
}