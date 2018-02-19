using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
namespace Zios.Extensions.Convert{
	using Zios.Extensions;
	public static class ConvertObject{
		public static List<Func<object,byte[]>> byteMethods = new List<Func<object,byte[]>>();
		public static List<Func<object,string>> serializeMethods = new List<Func<object,string>>();
		public static Type Convert<Type>(this object current){return System.Convert.ChangeType(current,typeof(Type)).As<Type>();}
		public static float ToFloat(this object current){return System.Convert.ChangeType(current,typeof(float)).As<float>();}
		public static int ToInt(this object current){return System.Convert.ChangeType(current,typeof(int)).As<int>();}
		public static double ToDouble(this object current){return System.Convert.ChangeType(current,typeof(double)).As<double>();}
		public static string ToString(this object current){return System.Convert.ChangeType(current,typeof(string)).As<string>();}
		public static bool ToBool(this object current){return System.Convert.ChangeType(current,typeof(bool)).As<bool>();}
		public static byte[] ToBytes(this object current){
			foreach(var custom in ConvertObject.byteMethods){
				var result = custom(current);
				if(!result.IsNull()){return result;}
			}
			if(current is float){return current.As<float>().ToBytes();}
			else if(current is int){return current.As<int>().ToBytes();}
			else if(current is bool){return current.As<bool>().ToBytes();}
			else if(current is string){return current.As<string>().ToStringBytes();}
			else if(current is byte){return current.As<byte>().ToBytes();}
			else if(current is short){return current.As<short>().ToBytes();}
			else if(current is double){return current.As<double>().ToBytes();}
			return new byte[0];
		}
		public static byte[] CreateHash<T>(this T current) where T : class{
			using(MemoryStream stream = new MemoryStream()){
				using(SHA512Managed hash = new SHA512Managed()){
					XmlSerializer serialize = new XmlSerializer(typeof(T));
					serialize.Serialize(stream,current);
					return hash.ComputeHash(stream);
				}
			}
		}
		public static string SerializeAuto(this object current){
			foreach(var custom in ConvertObject.serializeMethods){
				var result = custom(current);
				if(!result.IsNull()){return result;}
			}
			if(current is float){return current.As<float>().Serialize();}
			else if(current is int){return current.As<int>().Serialize();}
			else if(current is bool){return current.As<bool>().Serialize();}
			else if(current is string){return current.As<string>().Serialize();}
			else if(current is byte){return current.As<byte>().Serialize();}
			else if(current is short){return current.As<short>().Serialize();}
			else if(current is double){return current.As<double>().Serialize();}
			else if(current is ICollection){return current.As<Array>().Cast<object>().Serialize();}
			return current.ToString();
		}
		//============================
		// Casts
		//============================
		public static object Box<T>(this T current){
			return current.AsBox();
		}
		public static object AsBox<T>(this T current){
			return (object)current;
		}
		public static object[] AsBoxedArray<T>(this T current){
			return new object[]{current};
		}
		public static List<object> AsBoxedList<T>(this T current){
			return new List<object>{(object)current};
		}
		public static T As<T>(this object current){
			if(current.IsNull()){return default(T);}
			return (T)current;
		}
		public static T[] AsArray<T>(this T current){
			if(current.IsNull()){return new T[0];}
			return new T[]{current};
		}
		public static T[] AsArray<T>(this T current,int amount){
			return current.AsList().ToArray();
		}
		public static List<T> AsList<T>(this T current){
			if(current.IsNull()){return new List<T>();}
			return new List<T>{current};
		}
		public static List<T> AsList<T>(this T current,int amount){
			if(current.IsNull()){return new List<T>();}
			var collection = new List<T>();
			while(amount > 0){
				collection.Add(current);
				amount -= 1;
			}
			return collection;
		}
		//============================
		// Other
		//============================
		public static string GetClassName(this object current){
			string path = current.GetClassPath();
			if(path.Contains(".")){
				return path.Split(".").Last();
			}
			return path;
		}
		public static string GetClassPath(this object current){
			return current.GetType().ToString();
		}
	}
}