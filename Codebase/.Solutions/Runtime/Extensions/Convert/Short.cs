using System;
namespace Zios.Extensions.Convert{
	public static class ConvertShort{
		public static bool ToBool(this short current){return current != 0;}
		public static byte ToByte(this short current){return (byte)current;}
		public static int ToInt(this short current){return (int)current;}
		public static byte[] ToBytes(this short current){return BitConverter.GetBytes(current);}
		public static string Serialize(this short current,bool ignoreDefault=false,short defaultValue=0){
			return ignoreDefault && current == defaultValue ? "" : current.ToString();
		}
		public static short Deserialize(this short current,string value){return value.ToShort();}
	}
}