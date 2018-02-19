using UnityEngine;
namespace Zios.Unity.Extensions.Convert{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	public static class ConvertRect{
		//============================
		// From
		//============================
		//============================
		// To
		//============================
		public static Rect ToRect(this string current,string separator=","){
			var values = current.Split(separator).ConvertAll<float>();
			return new Rect(values[0],values[1],values[2],values[3]);
		}
		public static Rect ToRect(this float[] current){
			Rect result = new Rect();
			result.x = current.Length >= 1 ? current[0] : 0;
			result.y = current.Length >= 2 ? current[1] : 0;
			result.width = current.Length >= 3 ? current[2] : 0;
			result.height = current.Length >= 4 ? current[3] : 0;
			return result;
		}
	}
}