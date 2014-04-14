using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Serialization;
public static class ObjectExtension{
	public static T Cast<T>(this object current,ref T type){
		return (T)Convert.ChangeType(current,typeof(T));
	}
	public static T Cast<T>(this object current){
		return (T)Convert.ChangeType(current,typeof(T));
	}
	public static T[] CastArray<T>(this object current){
		return ((Array)current).Convert<T>();
	}
	public static T Clone<T>(this T target) where T : class{
		if(target == null){
			return null;
		}
		MethodInfo method = target.GetType().GetMethod("MemberwiseClone",BindingFlags.Instance|BindingFlags.NonPublic);
		if(method != null){
			return (T)method.Invoke(target,null);
		}
		else{
			return null;
		}
	}
	public static bool HasMethod(this object current,string name,BindingFlags flags = BindingFlags.Instance|BindingFlags.Public,Type type = null){
		Type currentType = type == null ? current.GetType() : type;
		return currentType.GetMethod(name,flags) != null;
	}
	public static bool HasAttribute(this object current,string name){
		bool hasProperty = current.GetType().GetProperty(name) != null;
		bool hasField = current.GetType().GetField(name) != null;
		return hasProperty || hasField;
	}
	public static MethodInfo GetMethod(this object current,string name,BindingFlags flags = BindingFlags.Instance|BindingFlags.Public,Type type = null){
		Type currentType = type == null ? current.GetType() : type;
		return currentType.GetMethod(name,flags);
	}
	public static T GetAttribute<T>(this object current,string name){
		Type type = current.GetType();
		PropertyInfo property = type.GetProperty(name);
		FieldInfo field = type.GetField(name);
		if(property != null){
			return (T)property.GetValue(current,null);
		}
		if(field != null){
			return (T)field.GetValue(current);
		}
		return default(T);
	}
	public static List<string> ListAttributes(this object current,List<Type> limitTypes = null){
		List<string> attributes = new List<string>();
		foreach(FieldInfo field in current.GetType().GetFields()){
			if(limitTypes != null){
				if(limitTypes.Contains(field.FieldType)){
					attributes.Add(field.Name);
				}
			}
			else{
				attributes.Add(field.Name);
			}
		}
		foreach(PropertyInfo property in current.GetType().GetProperties()){
			if(limitTypes != null){
				if(limitTypes.Contains(property.PropertyType)){
					attributes.Add(property.Name);
				}
			}
			else{
				attributes.Add(property.Name);
			}
		}
		return attributes;
	}
	public static List<string> ListMethods(this object current,List<Type> argumentTypes = null,BindingFlags flags = BindingFlags.Instance|BindingFlags.Public,Type type = null){
		List<string> methods = new List<string>();
		Type currentType = type == null ? current.GetType() : type;
		foreach(MethodInfo method in currentType.GetMethods(flags)){
			if(argumentTypes != null){
				ParameterInfo[] parameters = method.GetParameters();
				bool match = parameters.Length == argumentTypes.Count;
				if(match){
					for(int i = 0;i < parameters.Length;i++){
						if(!parameters[i].ParameterType.Equals(argumentTypes[i])){
							match = false;
							break;
						}
					}
				}
				if(match){
					methods.Add(method.Name);
				}
			}
			else{
				methods.Add(method.Name);
			}
		}
		return methods;
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
	public static Type LoadType(this object current,string typeName){
		System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach(var A in AS){
			System.Type[] types = A.GetTypes();
			foreach(var T in types){
				if(T.FullName == typeName){
					return T;
				}
			}
		}
		return null;
	}
}