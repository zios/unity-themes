using System;
using System.Collections;
namespace Zios.Extensions{
	public static class ObjectExtensions{
		public static T Real<T>(this T current){
			if(current.IsNull()){return default(T);}
			return current;
		}
		public static T Real<T>(this object current){
			if(current.IsNull()){return default(T);}
			return (T)current;
		}
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
		public static bool IsStatic(this object current){
			Type type = current is Type ? (Type)current : current.GetType();
			return type.IsStatic();
		}
		public static bool IsGeneric(this object current){
			Type type = current is Type ? (Type)current : current.GetType();
			return type.ContainsGenericParameters || type.IsGenericType;
		}
		public static bool IsAny<A,B>(this object current){return current.Is<A>() || current.Is<B>();}
		public static bool IsAny<A,B,C>(this object current){return current.Is<A>() || current.Is<B>() || current.Is<C>();}
		public static bool IsAny<A,B,C,D>(this object current){return current.Is<A>() || current.Is<B>() || current.Is<C>() || current.Is<D>();}
		public static bool IsAny<A,B,C,D,E>(this object current){return current.Is<A>() || current.Is<D>() || current.Is<C>() || current.Is<D>() || current.Is<E>();}
		public static bool Is<T>(this object current){
			if(current.IsNull()){return false;}
			var type = current is Type ? (Type)current : current.GetType();
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
	}
}