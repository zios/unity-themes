using System;
using System.Collections.Generic;
namespace Zios.Extensions.Convert{
	public static class ConvertInt{
		public static string ToHex(this int current){return current.ToString("X6");}
		public static Enum ToEnum(this int current,Type enumType){return (Enum)Enum.ToObject(enumType,current);}
		public static T ToEnum<T>(this int current){return (T)Enum.ToObject(typeof(T),current);}
		public static bool ToBool(this int current){return current != 0;}
		public static byte ToByte(this int current){return (byte)current;}
		public static short ToShort(this int current){return (short)current;}
		public static byte[] ToBytes(this int current){return BitConverter.GetBytes(current);}
		public static string Serialize(this int current,bool ignoreDefault=false,int defaultValue=0){
			return ignoreDefault && current == defaultValue ? "" : current.ToString();
		}
		public static bool[] ToFlags(this int current){
			var value = 1;
			var result = new List<bool>();
			while(value <= current){
				result.Add((value & current) != 0);
				value *= 2;
			}
			return result.ToArray();
		}
		public static bool[] ToFlags(this int current,int size){
			var value = 1;
			var result = new bool[size];
			for(int index=0;index<size;++index){
				result[index] = ((value & current) != 0);
				value *= 2;
			}
			return result;
		}
		public static int Deserialize(this int current,string value){return value.ToInt();}
	}
}