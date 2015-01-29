using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Serialization;
public static class ObjectExtension{
	const BindingFlags allFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
	const BindingFlags privateFlags = BindingFlags.Instance|BindingFlags.NonPublic;
	const BindingFlags publicFlags = BindingFlags.Instance|BindingFlags.Public;
	public static T Cast<T>(this object current,T type){
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
		MethodInfo method = target.GetType().GetMethod("MemberwiseClone",privateFlags);
		if(method != null){
			return (T)method.Invoke(target,null);
		}
		else{
			return null;
		}
	}
	public static object CallMethod(this object current,string name,object[] parameters=null){
		return current.CallMethod<object>(name,parameters);
	}
	public static V CallMethod<V>(this object current,string name,object[] parameters=null){
		if(current.IsStatic()){
			return (V)current.GetMethod(name).Invoke(null,parameters);
		}
		return (V)current.GetMethod(name).Invoke(current,parameters);
	}
	public static bool HasMethod(this object current,string name,BindingFlags flags = allFlags){
		Type type = current is Type ? (Type)current : current.GetType();
		return type.GetMethod(name,flags) != null;
	}
	public static bool HasVariable(this object current,string name,BindingFlags flags = allFlags){
		Type type = current is Type ? (Type)current : current.GetType();
		bool hasProperty = type.GetProperty(name,flags) != null;
		bool hasField = type.GetField(name,flags) != null;
		return hasProperty || hasField;
	}
	public static MethodInfo GetMethod(this object current,string name,BindingFlags flags = allFlags){
		Type type = current is Type ? (Type)current : current.GetType();
		return type.GetMethod(name,flags);
	}
	public static object GetVariable(this object current,string name,int index=-1,BindingFlags flags = allFlags){
		return current.GetVariable<object>(name,index,flags);
	}
	public static T GetVariable<T>(this object current,string name,int index=-1,BindingFlags flags = allFlags){
		Type type = current is Type ? (Type)current : current.GetType();
		object instance = current.IsStatic() || current is Type ? null : current;
		PropertyInfo property = type.GetProperty(name,flags);
		FieldInfo field = type.GetField(name,flags);
		if(index != -1){
			if(current is Vector3){
				//return current.Cast<Vector3>()[index].Cast<object>().Cast<T>();
				return (T)((object)(((Vector3)current)[index]));
			}
			IList list = (IList)field.GetValue(instance);
			return (T)list[index];
		}
		if(property != null){
			return (T)property.GetValue(instance,null);
		}
		if(field != null){
			return (T)field.GetValue(instance);
		}
		return default(T);
	}
	public static Type GetVariableType(this object current,string name,int index=-1,BindingFlags flags = allFlags){
		Type type = current is Type ? (Type)current : current.GetType();
		PropertyInfo property = type.GetProperty(name,flags);
		FieldInfo field = type.GetField(name,flags);
		if(index != -1){
			if(current is Vector3){return typeof(float);}
			IList list = (IList)field.GetValue(current);
			return list[index].GetType();
		}
		if(property != null){return property.PropertyType;}
		if(field != null){return field.FieldType;}
		return typeof(Type);
	}
	public static void SetVariable<T>(this object current,string name,T value,int index=-1){
		Type type = current is Type ? (Type)current : current.GetType();
		PropertyInfo property = type.GetProperty(name);
		FieldInfo field = type.GetField(name);
		if(index != -1){
			if(current is Vector3){
				Vector3 currentVector3 = (Vector3)current;
				currentVector3[index] = (float)Convert.ChangeType(value,typeof(float));
			}
			Array currentArray = (Array)current;
			currentArray.SetValue(value,index);
		}
		if(property != null){
			property.SetValue(current,value,null);
		}
		if(field != null){
			field.SetValue(current,value);
		}
	}
	public static List<string> ListVariables(this object current,List<Type> limitTypes = null){
		List<string> variables = new List<string>();
		foreach(FieldInfo field in current.GetType().GetFields()){
			if(limitTypes != null){
				if(limitTypes.Contains(field.FieldType)){
					variables.Add(field.Name);
				}
			}
			else{
				variables.Add(field.Name);
			}
		}
		foreach(PropertyInfo property in current.GetType().GetProperties()){
			if(limitTypes != null){
				if(limitTypes.Contains(property.PropertyType)){
					variables.Add(property.Name);
				}
			}
			else{
				variables.Add(property.Name);
			}
		}
		return variables;
	}
	public static List<string> ListMethods(this object current,List<Type> argumentTypes = null,BindingFlags flags = allFlags,Type type = null){
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
	public static bool IsEmpty(this object current){
        return current == null || current.Equals(null) || (current is string && ((string)current).IsEmpty());
    }
	public static bool IsNull(this object current){
        return current == null || current.Equals(null);
    }
	public static bool IsType(this object current,Type value){
		return current.GetType().IsType(value);
	}
	public static T[] AsArray<T>(this T current){
		return new T[]{current};
	}
	public static object[] AsBoxedArray<T>(this T current){
		return new object[]{current};
	}
	public static List<T> AsList<T>(this T current){
		return new List<T>{current};
	}
	public static bool IsStatic(this object current){return current.GetType().IsStatic();}
}