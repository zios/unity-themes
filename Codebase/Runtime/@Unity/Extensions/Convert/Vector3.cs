using System.Linq;
using UnityEngine;
namespace Zios.Unity.Extensions.Convert{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.SystemAttributes;
	[InitializeOnLoad]
	public static class ConvertVector3{
		static ConvertVector3(){Setup();}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Setup(){
			ConvertObject.serializeMethods.Add((current,separator,changesOnly)=>{
				return current is Vector3 ? current.As<Vector3>().Serialize() : null;
			});
			ConvertObject.byteMethods.Add((current)=>{
				return current is Vector3 ? current.As<Vector3>().ToBytes() : null;
			});
			ConvertString.deserializeMethods.Add((type,current,separator)=>{
				return type == typeof(Vector3) ? Vector3.zero.Deserialize(current).Box() : null;
			});
		}
		//============================
		// From
		//============================
		public static string Serialize(this Vector3 current,bool ignoreDefault=false,Vector3 defaultValue=default(Vector3)){
			return ignoreDefault && current == defaultValue ? "" : current.ToString();
		}
		public static byte[] ToBytes(this Vector3 current){return current.x.ToBytes().Append(current.y).Append(current.z);}
		public static string ToString(this Vector3 current){return "("+current.x+","+current.y+","+current.z+")";}
		public static Vector3 ToRadian(this Vector3 vector){
			Vector3 copy = vector;
			copy.x = vector.x / 360.0f;
			copy.y = vector.y / 360.0f;
			copy.z = vector.z / 360.0f;
			return copy;
		}
		public static Quaternion ToRotation(this Vector3 current){
			return Quaternion.Euler(current[1],current[0],current[2]);
		}
		public static float[] ToFloatArray(this Vector3 current){
			return new float[3]{current.x,current.y,current.z};
		}
		public static Color ToColor(this Vector3 current){return new Color(current.x,current.y,current.z);}
		//============================
		// To
		//============================
		public static Vector3 Deserialize(this Vector3 current,string value){return value.ToVector3();}
		public static Vector3 ToVector3(this string current,string separator=","){
			if(!current.Contains(separator)){return Vector3.zero;}
			var values = current.Trim("(",")").Split(separator).ConvertAll<float>().ToArray();
			return new Vector3(values[0],values[1],values[2]);
		}
		public static Vector3 ToVector3(this byte[] current){return current.ReadVector3();}
		public static Vector3 ToVector3(this float[] current){
			float x = current.Length >= 1 ? current[0] : 0;
			float y = current.Length >= 2 ? current[1] : 0;
			float z = current.Length >= 3 ? current[2] : 0;
			return new Vector3(x,y,z);
		}
		public static Vector3 ReadVector3(this byte[] current,int index=0){
			float x = current.ReadFloat(index);
			float y = current.ReadFloat(index+4);
			float z = current.ReadFloat(index+8);
			return new Vector3(x,y,z);
		}
	}
}