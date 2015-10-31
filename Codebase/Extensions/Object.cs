using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Serialization;
namespace Zios{
	public static class ObjectExtension{
		public const BindingFlags allFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		public const BindingFlags staticFlags = BindingFlags.Static|BindingFlags.Public;
		public const BindingFlags instanceFlags = BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		public const BindingFlags privateFlags = BindingFlags.Instance|BindingFlags.NonPublic;
		public const BindingFlags publicFlags = BindingFlags.Instance|BindingFlags.Public;
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
		//=========================
		// Reflection - Methods
		//=========================
		public static object CallExactMethod(this object current,string name,params object[] parameters){
			return current.CallExactMethod<object>(name,allFlags,parameters);
		}
		public static V CallExactMethod<V>(this object current,string name,params object[] parameters){
			return current.CallExactMethod<V>(name,allFlags,parameters);
		}
		public static V CallExactMethod<V>(this object current,string name,BindingFlags flags,params object[] parameters){
			List<Type> argumentTypes = new List<Type>();
			foreach(var parameter in parameters){
				argumentTypes.Add(parameter.GetType());
			}
			var methods = current.GetMethods(argumentTypes,name,flags);
			if(methods.Count < 1){
				Debug.LogWarning("[Object] No method found to call -- " + name);
				return default(V);
			}
			if(current.IsStatic() || current is Type){
				return (V)methods[0].Invoke(null,parameters);
			}
			return (V)methods[0].Invoke(current,parameters);
		}
		public static object CallMethod(this object current,string name,params object[] parameters){
			return current.CallMethod<object>(name,allFlags,parameters);
		}
		public static V CallMethod<V>(this object current,string name,params object[] parameters){
			return current.CallMethod<V>(name,allFlags,parameters);
		}
		public static V CallMethod<V>(this object current,string name,BindingFlags flags,params object[] parameters){
			var method = current.GetMethod(name,flags);
			if(method == null){
				Debug.LogWarning("[Object] No method found to call -- " + name);
				return default(V);
			}
			if(current.IsStatic() || current is Type){
				return (V)method.Invoke(null,parameters);
			}
			return (V)method.Invoke(current,parameters);
		}
		public static bool HasMethod(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			return type.GetMethod(name,flags) != null;
		}
		public static MethodInfo GetMethod(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			return type.GetMethod(name,flags);
		}
		public static List<MethodInfo> GetMethods(this object current,List<Type> argumentTypes=null,string name="",BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			List<MethodInfo> methods = new List<MethodInfo>();
			foreach(MethodInfo method in type.GetMethods(flags)){
				if(!name.IsEmpty() && !method.Name.Matches(name,true)){continue;}
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
					if(!match){continue;}
				}
				methods.Add(method);
			}
			return methods;
		}
		public static List<string> ListMethods(this object current,List<Type> argumentTypes=null,BindingFlags flags=allFlags){
			return current.GetMethods(argumentTypes,"",flags).Select(x=>x.Name).ToList();
		}
		//=========================
		// Reflection - Attributes
		//=========================
		public static bool HasAttribute(this object current,string name,Type attribute){
			return current.ListAttributes(name).Exists(x=>x.GetType()==attribute);
		}
		public static System.Attribute[] ListAttributes(this object current,string name){
			Type type = current is Type ? (Type)current : current.GetType();
			var property = type.GetProperty(name,allFlags);
			var field = type.GetField(name,allFlags);
			System.Attribute[] attributes = new System.Attribute[0];
			if(field != null){attributes = System.Attribute.GetCustomAttributes(field);}
			if(property != null){attributes = System.Attribute.GetCustomAttributes(property);}
			return attributes;
		}
		//=========================
		// Reflection - Variables
		//=========================
		public static bool HasVariable(this object current,string name,BindingFlags flags = allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			bool hasProperty = type.GetProperty(name,flags) != null;
			bool hasField = type.GetField(name,flags) != null;
			return hasProperty || hasField;
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
		public static void SetVariable<T>(this object current,string name,T value,int index=-1,BindingFlags flags = allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			current = current.IsStatic() ? null : current;
			PropertyInfo property = type.GetProperty(name,flags);
			FieldInfo field = type.GetField(name,flags);
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
		public static Dictionary<string,object> GetVariables(this object current,List<Type> onlyTypes = null,List<Type> withoutAttributes = null,BindingFlags flags = allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			object instance = current.IsStatic() || current is Type ? null : current;
			Dictionary<string,object> variables = new Dictionary<string,object>();
			foreach(FieldInfo field in type.GetFields(flags)){
				if(onlyTypes != null && !onlyTypes.Contains(field.FieldType)){continue;}
				if(withoutAttributes != null){
					var attributes = System.Attribute.GetCustomAttributes(field);
					if(attributes.Any(x=>withoutAttributes.Any(y=>y==x.GetType()))){continue;}
					//if(attributes.Intersect(limitAttributes).Any()){continue;}
				}
				try{variables[field.Name] = field.GetValue(instance);}
				catch{}
			}
			foreach(PropertyInfo property in type.GetProperties(flags)){
				if(onlyTypes != null && !onlyTypes.Contains(property.PropertyType)){continue;}
				if(withoutAttributes != null){
					var attributes = System.Attribute.GetCustomAttributes(property);
					if(attributes.Any(x=>withoutAttributes.Any(y=>y==x.GetType()))){continue;}
				}
				try{variables[property.Name] = property.GetValue(instance,null);}
				catch{}
			}
			return variables;
		}
		public static List<string> ListVariables(this object current,List<Type> onlyTypes = null,List<Type> withoutAttributes = null,BindingFlags flags = allFlags){
			return current.GetVariables(onlyTypes,withoutAttributes,flags).Keys.ToList();
		}
		//=========================
		// Shortcuts - Checks
		//=========================
		public static bool IsEmpty(this object current){
			return current == null || current.Equals(null) || (current is string && ((string)current).IsEmpty());
		}
		public static bool IsNull(this object current){
			return current == null || current.Equals(null);
		}
		public static bool IsStatic(this object current){
			Type type = current is Type ? (Type)current : current.GetType();
			return type.IsStatic();
		}
		//=========================
		// Shortcuts - Casts
		//=========================
		public static T ChangeType<T>(this object current,T type){
			return (T)Convert.ChangeType(current,typeof(T));
		}
		public static T ChangeType<T>(this object current){
			return (T)Convert.ChangeType(current,typeof(T));
		}
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
			return (T)current;
		}
		public static T[] AsArray<T>(this T current){
			return new T[]{current};
		}
		public static List<T> AsList<T>(this T current){
			return new List<T>{current};
		}
		//=========================
		// Other
		//=========================
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