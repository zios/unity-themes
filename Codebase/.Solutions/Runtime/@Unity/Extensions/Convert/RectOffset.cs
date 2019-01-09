using UnityEngine;
namespace Zios.Unity.Extensions.Convert{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	public static class ConvertRectOffset{
		//============================
		// From
		//============================
		public static string Serialize(this RectOffset current,string separator=" "){
			return current.left+separator+current.right+separator+current.top+separator+current.bottom;
		}
		//============================
		// To
		//============================
		public static RectOffset ToRectOffset(this string current,string separator=" "){
			var values = current.Split(separator).ConvertAll<int>();
			return new RectOffset(values[0],values[1],values[2],values[3]);
		}
	}
}