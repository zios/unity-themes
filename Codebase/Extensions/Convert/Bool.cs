using System;
namespace Zios.Extensions.Convert{
	public static class ConvertBool{
		public static int ToInt(this bool current){return current ? 1 : 0;}
		public static byte ToByte(this bool current){return current.ToInt().ToByte();}
		public static byte[] ToBytes(this bool current){return BitConverter.GetBytes(current);}
		public static string Serialize(this bool current){return current.ToInt().ToString();}
		public static bool Deserialize(this bool current,string value){return value.ToBool();}
	}
}