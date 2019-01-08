using System.Linq;
using UnityEngine;
namespace Zios.Unity.Extensions.Convert{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	public static class ConvertVector4{
		//============================
		// From
		//============================
		public static string Serialize(this Vector4 current){return current.ToString();}
		public static string ToString(this Vector4 current){return "("+current.x+","+current.y+","+current.z+","+current.w+")";}
		public static byte[] ToBytes(this Vector4 current){return current.x.ToBytes().Append(current.y).Append(current.z).Append(current.w);}
		public static float[] ToFloatArray(this Vector4 current){
			return new float[4]{current.x,current.y,current.z,current.w};
		}
		public static Color ToColor(this Vector4 current){return new Color(current.x,current.y,current.z,current.w);}
		//============================
		// To
		//============================
		public static Vector4 Deserialize(this Vector4 current,string value){return value.ToVector4();}
		public static Vector4 ToVector4(this float[] current){
			float x = current.Length >= 1 ? current[0] : 0;
			float y = current.Length >= 2 ? current[1] : 0;
			float z = current.Length >= 3 ? current[2] : 0;
			float w = current.Length >= 4 ? current[3] : 0;
			return new Vector4(x,y,z,w);
		}
		public static Vector4 ToVector4(this string current,string separator=","){
			if(!current.Contains(separator)){return Vector4.zero;}
			var values = current.Trim("(",")").Split(separator).ConvertAll<float>().ToArray();
			return new Vector4(values[0],values[1],values[2],values[3]);
		}
	}
}