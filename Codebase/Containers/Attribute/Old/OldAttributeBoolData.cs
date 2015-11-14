using UnityEngine;
using Zios;
namespace Zios{
	[AddComponentMenu("")]
	public class OldAttributeBoolData : OldAttributeData{
		public bool value;
		public override AttributeData Convert(){
			var data = new AttributeBoolData();
			data.value = this.value;
			data.rawValue = this.value.ToString();
			data.rawType = typeof(AttributeBoolData).FullName;
			return data;
		}
	}
}