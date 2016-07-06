namespace Zios{
	public static class ByteExtension{
		//=====================
		// Conversion
		//=====================
		public static int ToInt(this byte current){return (int)current;}
		public static char ToChar(this byte current){return (char)current;}
		public static string ToString(this byte current){return current.ToChar().ToString();}
		public static float ToFloat(this byte current){return (float)current;}
		public static double ToDouble(this byte current){return (double)current;}
		public static bool ToBool(this byte current){return current.ToInt().ToBool();}
		public static byte[] ToBytes(this byte current){return new byte[1]{current};}
		public static string Serialize(this byte current){return current.ToString();}
		public static short Deserialize(this byte current,string value){return value.ToByte();}
	}
}