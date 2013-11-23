using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Serialization;
public static class ObjectExtension{
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
		MethodInfo method = target.GetType().GetMethod("MemberwiseClone",BindingFlags.Instance | BindingFlags.NonPublic);
		if(method != null){
			return (T)method.Invoke(target,null);
		}
		else{
			return null;
		}
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
	public static List<string> ListMethods(this object current){
		List<string> methods = new List<string>();
		foreach(MethodInfo method in current.GetType().GetMethods()){
			methods.Add(method.Name);
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
}