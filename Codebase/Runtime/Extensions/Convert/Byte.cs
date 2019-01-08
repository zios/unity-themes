namespace Zios.Extensions.Convert{
	public static class ConvertByte{
		public static float ToFloat(this byte current){return (float)current;}
		public static int ToInt(this byte current){return (int)current;}
		public static char ToChar(this byte current){return (char)current;}
		public static string ToString(this byte current){return current.ToChar().ToString();}
		public static double ToDouble(this byte current){return (double)current;}
		public static bool ToBool(this byte current){return current.ToInt().ToBool();}
		public static byte[] ToBytes(this byte current){return new byte[1]{current};}
		public static string Serialize(this byte current,bool ignoreDefault=false,byte defaultValue=0){
			return ignoreDefault && current == defaultValue ? "" : current.ToString();
		}
		public static byte Deserialize(this byte current,string value){return value.ToByte();}
	}
}