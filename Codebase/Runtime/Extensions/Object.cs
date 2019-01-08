using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
namespace Zios.Extensions{
	public static class ObjectExtensions{
		//============================
		// Checks
		//============================
		public static bool IsDefault<T>(this T current){
			return current == null || current.Equals(default(T));
		}
		public static bool IsEmpty(this object current){
			bool isEmptyString = (current is string && ((string)current).IsEmpty());
			bool isEmptyCollection = (current is IList && ((IList)current).Count == 0);
			return current.IsNull() || isEmptyCollection || isEmptyString;
		}
		public static bool IsNumber(this object current){
			bool isByte = current is sbyte || current is byte;
			bool isInteger = current is short || current is ushort || current is int || current is uint || current is long || current is ulong;
			bool isDecimal = current is float || current is double || current is decimal;
			return isInteger || isDecimal || isByte;
		}
		public static bool IsNull(this object current){
			return current == null || current.Equals(null);
		}
		public static bool IsStatic(this object current){return current.AsType().IsStatic();}
		public static bool IsGeneric(this object current){
			var type = current.AsType();
			return type.ContainsGenericParameters || type.IsGenericType;
		}
		public static bool IsEnum(this object current){return current.AsType().IsEnum;}
		public static bool IsArray(this object current){return current.AsType().IsArray;}
		public static bool IsList(this object current){return current is IList && current.AsType().IsGenericType;}
		public static bool IsDictionary(this object current){return current.AsType().IsGenericType && current.AsType().GetGenericTypeDefinition() == typeof(Dictionary<,>);}
		public static bool IsAny<A,B>(this object current){return current.Is<A>() || current.Is<B>();}
		public static bool IsAny<A,B,C>(this object current){return current.Is<A>() || current.Is<B>() || current.Is<C>();}
		public static bool IsAny<A,B,C,D>(this object current){return current.Is<A>() || current.Is<B>() || current.Is<C>() || current.Is<D>();}
		public static bool IsAny<A,B,C,D,E>(this object current){return current.Is<A>() || current.Is<D>() || current.Is<C>() || current.Is<D>() || current.Is<E>();}
		public static bool Is<T>(this object current){
			if(current.IsNull()){return false;}
			var type = current.AsType();
			return type.IsSubclassOf(typeof(T)) || type.IsAssignableFrom(typeof(T));
		}
		public static bool Is(this Type current,Type value){
			return current.IsSubclassOf(value) || current.IsAssignableFrom(value);
		}
		public static bool Is<T>(this T current,Type value){
			return typeof(T).IsSubclassOf(value) || typeof(T).IsAssignableFrom(value);
		}
		public static bool Is<T>(this T current,string name){
			var type = typeof(T);
			var value = Type.GetType(name);
			if(value.IsNull()){
				System.Console.WriteLine("[ObjectExtension] Type -- " + name + " not found.");
				return false;
			}
			return type.IsSubclassOf(value) || type.IsAssignableFrom(value);
		}
		public static bool IsNot<T>(this T current,Type value){return !current.Is(value);}
		public static bool IsNot<T>(this T current,string name){return !current.Is(name);}
		public static bool IsNot<T>(this object current){return !current.Is<T>();}
		//============================
		// Other
		//============================
		public static Type AsType(this object current){
            if (current == null) return typeof(Type);
            return current is Type ? (Type)current : current.GetType();
        }
		public static Type Default<Type>(this Type current){return default(Type);}
		public static byte[] CreateHash<T>(this T current) where T : class{
			using(MemoryStream stream = new MemoryStream()){
				using(SHA512Managed hash = new SHA512Managed()){
					XmlSerializer serialize = new XmlSerializer(typeof(T));
					serialize.Serialize(stream,current);
					return hash.ComputeHash(stream);
				}
			}
		}
	}
}