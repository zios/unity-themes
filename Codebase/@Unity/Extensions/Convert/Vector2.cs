using System.Linq;
using UnityEngine;
namespace Zios.Unity.Extensions.Convert{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	public static class ConvertVector2{
		//============================
		// From
		//============================
		//============================
		// To
		//============================
		public static Vector2 ToVector2(this float[] current){
			float x = current.Length >= 1 ? current[0] : 0;
			float y = current.Length >= 2 ? current[1] : 0;
			return new Vector2(x,y);
		}
		public static Vector2 ToVector2(this string current,string separator=","){
			if(!current.Contains(separator)){return Vector2.zero;}
			var values = current.Trim("(",")").Split(separator).ConvertAll<float>().ToArray();
			return new Vector2(values[0],values[1]);
		}
	}
}