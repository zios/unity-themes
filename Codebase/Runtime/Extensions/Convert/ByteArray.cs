using System;
using System.Text;
namespace Zios.Extensions.Convert{
	using Zios.Extensions;
	public static class ConvertByteArray{
		public static byte[] Append(this byte[] current,object value){
			current = current.Concat(value.ToBytes());
			return current;
		}
		public static byte[] Prepend(this byte[] current,object value){
			current = value.ToBytes().Concat(current);
			return current;
		}
		public static int ToInt(this byte[] current){return BitConverter.ToInt32(current,0);}
		public static short ToShort(this byte[] current){return BitConverter.ToInt16(current,0);}
		public static float ToFloat(this byte[] current){return BitConverter.ToSingle(current,0);}
		public static double ToDouble(this byte[] current){return BitConverter.ToDouble(current,0);}
		public static bool ToBool(this byte[] current){return current[0] == 1 ? true : false;}
		public static char ToChar(this byte[] current){return BitConverter.ToChar(current,0);}
		public static string ToUTFString(this byte[] current){return Encoding.UTF8.GetString(current);}
		public static string SerializeString(this byte[] current){return current.ToUTFString();}
		public static byte[] DeserializeString(this byte[] current,string value){return value.ToStringBytes();}
		public static string Serialize(this byte[] current){return System.Convert.ToBase64String(current);}
		public static byte[] Deserialize(this byte[] current,string value){return System.Convert.FromBase64String(value);}
	}
}