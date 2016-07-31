using System;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Zios{
	public static class ByteArrayExtension{
		//=====================
		// Conversion
		//=====================
		public static int ReadInt(this byte[] current,int index=0){return BitConverter.ToInt32(current,index);}
		public static short ReadShort(this byte[] current,int index=0){return BitConverter.ToInt16(current,index);}
		public static float ReadFloat(this byte[] current,int index=0){return BitConverter.ToSingle(current,index);}
		public static double ReadDouble(this byte[] current,int index=0){return BitConverter.ToDouble(current,index);}
		public static bool ReadBool(this byte[] current,int index=0){return current[index] == 1 ? true : false;}
		public static char ReadChar(this byte[] current,int index=0){return BitConverter.ToChar(current,index);}
		public static string ReadString(this byte[] current,int index=0){return Encoding.UTF8.GetString(current.Skip(index).ToArray());}
		public static Vector3 ReadVector3(this byte[] current,int index=0){
			float x = current.ReadFloat(index);
			float y = current.ReadFloat(index+4);
			float z = current.ReadFloat(index+8);
			return new Vector3(x,y,z);
		}
		public static int ToInt(this byte[] current){return BitConverter.ToInt32(current,0);}
		public static short ToShort(this byte[] current){return BitConverter.ToInt16(current,0);}
		public static float ToFloat(this byte[] current){return BitConverter.ToSingle(current,0);}
		public static double ToDouble(this byte[] current){return BitConverter.ToDouble(current,0);}
		public static bool ToBool(this byte[] current){return current[0] == 1 ? true : false;}
		public static char ToChar(this byte[] current){return BitConverter.ToChar(current,0);}
		public static string ToUTFString(this byte[] current){return Encoding.UTF8.GetString(current);}
		public static Vector3 ToVector3(this byte[] current){return current.ReadVector3();}
		public static byte[] Append(this byte[] current,object value){
			current = current.Concat(value.ToBytes());
			return current;
		}
		public static byte[] Prepend(this byte[] current,object value){
			current = value.ToBytes().Concat(current);
			return current;
		}
		public static string SerializeString(this byte[] current){return current.ToUTFString();}
		public static byte[] DeserializeString(this byte[] current,string value){return value.ToStringBytes();}
		public static string Serialize(this byte[] current){return Convert.ToBase64String(current);}
		public static byte[] Deserialize(this byte[] current,string value){return Convert.FromBase64String(value);}
	}
}