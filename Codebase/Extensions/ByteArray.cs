using System;
using System.Linq;
using System.Text;
namespace Zios.Extensions{
	public static class ByteArrayExtension{
		public static int ReadInt(this byte[] current,int index=0){return BitConverter.ToInt32(current,index);}
		public static short ReadShort(this byte[] current,int index=0){return BitConverter.ToInt16(current,index);}
		public static float ReadFloat(this byte[] current,int index=0){return BitConverter.ToSingle(current,index);}
		public static double ReadDouble(this byte[] current,int index=0){return BitConverter.ToDouble(current,index);}
		public static bool ReadBool(this byte[] current,int index=0){return current[index] == 1 ? true : false;}
		public static char ReadChar(this byte[] current,int index=0){return BitConverter.ToChar(current,index);}
		public static string ReadString(this byte[] current,int index=0){return Encoding.UTF8.GetString(current.Skip(index).ToArray());}
	}
}
