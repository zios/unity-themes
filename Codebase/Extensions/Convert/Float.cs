using System;
namespace Zios.Extensions.Convert{
	public static class ConvertFloat{
		public static bool ToBool(this float current){return current != 0;}
		public static int ToInt(this float current){return (int)current;}
		public static byte ToByte(this float current){return (byte)current;}
		public static short ToShort(this float current){return (short)current;}
		public static byte[] ToBytes(this float current){return BitConverter.GetBytes(current);}
		public static string Serialize(this float current){return current.ToString();}
		public static float Deserialize(this float current,string value){return value.ToFloat();}
	}
}