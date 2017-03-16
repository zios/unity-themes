using UnityEngine;
namespace Zios{
	public static class Vector4Extension{
		//=====================
		// Conversion
		//=====================
		public static string ToString(this Vector4 current){return "("+current.x+","+current.y+","+current.z+","+current.w+")";}
		public static byte[] ToBytes(this Vector4 current){return current.ToBytes(false);}
		public static byte[] ToBytes(this Vector4 current,bool pack){
			if(pack){return Pack.PackFloats(current.x,current.y,current.z,current.w).ToBytes();}
			return current.x.ToBytes().Append(current.y).Append(current.z).Append(current.w);
		}
		public static float[] ToFloatArray(this Vector4 current){
			return new float[4]{current.x,current.y,current.z,current.w};
		}
		public static Color ToColor(this Vector4 current){return new Color(current.x,current.y,current.z,current.w);}
		public static string Serialize(this Vector4 current){return current.ToString();}
		public static Vector4 Deserialize(this Vector4 current,string value){return value.ToVector4();}
	}
}