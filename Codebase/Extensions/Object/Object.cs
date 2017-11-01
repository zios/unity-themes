using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Security.Cryptography;
using System.Xml.Serialization;
using UnityEngine;
namespace Zios{
	public static partial class ObjectExtension{
		//============================
		// Checks
		//============================
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
				Debug.Log("[ObjectExtension] Type -- " + name + " not found.");
				return false;
			}
			return type.IsSubclassOf(value) || type.IsAssignableFrom(value);
		}
		public static bool IsNot<T>(this T current,Type value){return !current.Is(value);}
		public static bool IsNot<T>(this T current,string name){return !current.Is(name);}
		public static bool IsNot<T>(this object current){return !current.Is<T>();}
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
		// Conversions
		//============================
		public static Type Convert<Type>(this object current){return System.Convert.ChangeType(current,typeof(Type)).As<Type>();}
		public static float ToFloat(this object current){return System.Convert.ChangeType(current,typeof(float)).As<float>();}
		public static int ToInt(this object current){return System.Convert.ChangeType(current,typeof(int)).As<int>();}
		public static double ToDouble(this object current){return System.Convert.ChangeType(current,typeof(double)).As<double>();}
		public static string ToString(this object current){return System.Convert.ChangeType(current,typeof(string)).As<string>();}
		public static bool ToBool(this object current){return System.Convert.ChangeType(current,typeof(bool)).As<bool>();}
		public static byte[] ToBytes(this object current){
			if(current is Vector3){return current.As<Vector3>().ToBytes();}
			else if(current is float){return current.As<float>().ToBytes();}
			else if(current is int){return current.As<int>().ToBytes();}
			else if(current is bool){return current.As<bool>().ToBytes();}
			else if(current is string){return current.As<string>().ToStringBytes();}
			else if(current is byte){return current.As<byte>().ToBytes();}
			else if(current is short){return current.As<short>().ToBytes();}
			else if(current is double){return current.As<double>().ToBytes();}
			return new byte[0];
		}
		public static string SerializeAuto(this object current){
			if(current is Texture2D){return current.As<Texture2D>().Serialize();}
			else if(current is GUIContent){return current.As<GUIContent>().Serialize();}
			else if(current is Vector3){return current.As<Vector3>().Serialize();}
			else if(current is Color){return current.As<Color>().Serialize();}
			else if(current is float){return current.As<float>().Serialize();}
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
		// Other
		//============================
		public static T Copy<T>(this T target) where T : class,new(){
			return new T().UseVariables(target);
		}
		public static T Clone<T>(this T target) where T : class{
			if(target.IsNull()){return null;}
			MethodInfo method = target.GetType().GetMethod("MemberwiseClone",privateFlags);
			if(method != null){
				return (T)method.Invoke(target,null);
			}
			return null;
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
		public static string GetAlias(this object current){
			if(current.HasVariable("alias")){return current.GetVariable<string>("alias");}
			//if(current.HasVariable("name")){return current.GetVariable<string>("name");}
			return current.GetType().Name;
		}
	}
}