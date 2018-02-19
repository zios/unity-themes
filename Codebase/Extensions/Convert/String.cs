using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
namespace Zios.Extensions.Convert{
	using Zios.Extensions;
	public static class ConvertString{
		public static List<Func<Type,string,object>> deserializeMethods = new List<Func<Type,string,object>>();
		public static string ToMD5(this string current){
			byte[] bytes = Encoding.UTF8.GetBytes(current);
			byte[] hash = MD5.Create().ComputeHash(bytes);
			return BitConverter.ToString(hash).Replace("-","");
		}
		public static short ToShort(this string current){
			if(current.IsEmpty()){return 0;}
			return System.Convert.ToInt16(current);
		}
		public static int ToInt(this string current){
			if(current.IsEmpty()){return 0;}
			return System.Convert.ToInt32(current);
		}
		public static float ToFloat(this string current){
			if(current.IsEmpty()){return 0;}
			return System.Convert.ToSingle(current);
		}
		public static double ToDouble(this string current){
			if(current.IsEmpty()){return 0;}
			return System.Convert.ToDouble(current);
		}
		public static bool ToBool(this string current){
			if(current.IsEmpty()){return false;}
			string lower = current.ToLower();
			return lower != "false" && lower != "f" && lower != "0";
		}
		public static T ToEnum<T>(this string current){
			return (T)Enum.Parse(typeof(T),current,true);
		}
		public static byte ToByte(this string current){return System.Convert.ToByte(current);}
		public static byte[] ToStringBytes(this string current){return Encoding.ASCII.GetBytes(current);}
		public static string Serialize(this string current){return current;}
		public static string Deserialize(this string current,string value){return value;}
		public static object Deserialize(this string current,Type type){
			foreach(var custom in ConvertString.deserializeMethods){
				var result = custom(type,current);
				if(!result.IsNull()){return result;}
			}
			if(type == typeof(float)){return new Single().Deserialize(current).Box();}
			else if(type == typeof(int)){return new Int32().Deserialize(current).Box();}
			else if(type == typeof(bool)){return new Boolean().Deserialize(current).Box();}
			else if(type == typeof(string)){return String.Empty.Deserialize(current).Box();}
			else if(type == typeof(byte)){return new Byte().Deserialize(current).Box();}
			else if(type == typeof(short)){return new Int16().Deserialize(current).Box();}
			else if(type == typeof(double)){return new Double().Deserialize(current).Box();}
			return default(Type);
		}
		public static Type Deserialize<Type>(this string current){
			foreach(var custom in ConvertString.deserializeMethods){
				var result = custom(typeof(Type),current);
				if(!result.IsNull()){return result.As<Type>();}
			}
			if(typeof(Type) == typeof(float)){return (Type)new Single().Deserialize(current).Box();}
			else if(typeof(Type) == typeof(int)){return (Type)new Int32().Deserialize(current).Box();}
			else if(typeof(Type) == typeof(bool)){return (Type)new Boolean().Deserialize(current).Box();}
			else if(typeof(Type) == typeof(string)){return (Type)String.Empty.Deserialize(current).Box();}
			else if(typeof(Type) == typeof(byte)){return (Type)new Byte().Deserialize(current).Box();}
			else if(typeof(Type) == typeof(short)){return (Type)new Int16().Deserialize(current).Box();}
			else if(typeof(Type) == typeof(double)){return (Type)new Double().Deserialize(current).Box();}
			else if(typeof(Type).IsCollection()){return (Type)new Type[0].Deserialize(current).Box();}
			return default(Type);
		}
	}
}