using System;
namespace Zios.Extensions.Convert{
	public static class ConvertDouble{
		public static bool ToBool(this double current){return current != 0;}
		public static int ToInt(this double current){return (int)current;}
		public static byte ToByte(this double current){return (byte)current;}
		public static float ToFloat(this double current){return (float)current;}
		public static byte[] ToBytes(this double current){return BitConverter.GetBytes(current);}
		public static string Serialize(this double current){return current.ToString();}
		public static double Deserialize(this double current,string value){return value.ToDouble();}
	}
}