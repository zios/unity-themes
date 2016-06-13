using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace Zios{
	using Containers;
	using Class = ObjectExtension;
	public static partial class ObjectExtension{
		[NonSerialized] public static bool debug;
		public static List<Type> emptyList = new List<Type>();
		public static Hierarchy<object,string,bool> warned = new Hierarchy<object,string,bool>();
		public static Hierarchy<Type,BindingFlags,IList<Type>,string,object> variables = new Hierarchy<Type,BindingFlags,IList<Type>,string,object>();
		public static Hierarchy<Type,BindingFlags,string,PropertyInfo> properties = new Hierarchy<Type,BindingFlags,string,PropertyInfo>();
		public static Hierarchy<Type,BindingFlags,string,FieldInfo> fields = new Hierarchy<Type,BindingFlags,string,FieldInfo>();
		public static Hierarchy<Type,BindingFlags,string,MethodInfo> methods = new Hierarchy<Type,BindingFlags,string,MethodInfo>();
		public const BindingFlags allFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		public const BindingFlags allFlatFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.FlattenHierarchy;
		public const BindingFlags staticFlags = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
		public const BindingFlags instanceFlags = BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		public const BindingFlags privateFlags = BindingFlags.Instance|BindingFlags.NonPublic;
		public const BindingFlags publicFlags = BindingFlags.Instance|BindingFlags.Public;
		//=========================
		// Methods
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
				if(ObjectExtension.debug){Debug.LogWarning("[Object] No method found to call -- " + name);}
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
				if(ObjectExtension.debug){Debug.LogWarning("[Object] No method found to call -- " + name);}
				return default(V);
			}
			if(current.IsStatic() || current is Type){
				return (V)method.Invoke(null,parameters);
			}
			return (V)method.Invoke(current,parameters);
		}
		public static bool HasMethod(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			return Class.GetMethod(type,name,flags) != null;
		}
		public static MethodInfo GetMethod(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			return Class.GetMethod(type,name,flags);
		}
		public static List<MethodInfo> GetMethods(this object current,IList<Type> argumentTypes=null,string name="",BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			List<MethodInfo> methods = new List<MethodInfo>();
			foreach(MethodInfo method in type.GetMethods(flags)){
				if(!name.IsEmpty() && !method.Name.Matches(name,true)){continue;}
				if(argumentTypes != null){
					ParameterInfo[] parameters = method.GetParameters();
					bool match = parameters.Length == argumentTypes.Count;
					if(match){
						for(int index=0;index<parameters.Length;++index){
							if(!argumentTypes[index].Is(parameters[index].ParameterType)){
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
		public static List<string> ListMethods(this object current,IList<Type> argumentTypes=null,BindingFlags flags=allFlags){
			return current.GetMethods(argumentTypes,"",flags).Select(x=>x.Name).ToList();
		}
		//=========================
		// Attributes
		//=========================
		public static bool HasAttribute(this object current,string name,Type attribute){
			return current.ListAttributes(name).Exists(x=>x.GetType()==attribute);
		}
		public static Attribute[] ListAttributes(this object current,string name){
			Type type = current is Type ? (Type)current : current.GetType();
			var property = type.GetProperty(name,allFlags);
			var field = type.GetField(name,allFlags);
			Attribute[] attributes = new Attribute[0];
			if(field != null){attributes = Attribute.GetCustomAttributes(field);}
			if(property != null){attributes = Attribute.GetCustomAttributes(property);}
			return attributes;
		}
		//=========================
		// Variables
		//=========================
		public static void ResetCache(){
			Class.properties.Clear();
			Class.variables.Clear();
			Class.methods.Clear();
			Class.fields.Clear();
		}
		public static PropertyInfo GetProperty(Type type,string name,BindingFlags flags=allFlags){
			var target = Class.properties.AddNew(type).AddNew(flags);
			if(!target.ContainsKey(name)){target[name] = type.GetProperty(name,flags);}
			return target[name];
		}
		public static FieldInfo GetField(Type type,string name,BindingFlags flags=allFlags){
			var target = Class.fields.AddNew(type).AddNew(flags);
			if(!target.ContainsKey(name)){target[name] = type.GetField(name,flags);}
			return target[name];
		}
		public static MethodInfo GetMethod(Type type,string name,BindingFlags flags=allFlags){
			var target = Class.methods.AddNew(type).AddNew(flags);
			if(!target.ContainsKey(name)){target[name] = type.GetMethod(name,flags);}
			return target[name];
		}
		public static bool HasVariable(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			bool hasProperty = Class.GetProperty(type,name,flags) != null;
			bool hasField = Class.GetField(type,name,flags) != null;
			return hasProperty || hasField;
		}
		public static Type GetVariableType(this object current,string name,int index=-1,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			var property = Class.GetProperty(type,name,flags);
			var field = Class.GetField(type,name,flags);
			if(index != -1){
				if(current is Vector3){return typeof(float);}
				IList list = (IList)field.GetValue(current);
				return list[index].GetType();
			}
			if(property != null){return property.PropertyType;}
			if(field != null){return field.FieldType;}
			return typeof(Type);
		}
		public static object GetVariable(this object current,string name,int index=-1,BindingFlags flags=allFlags){
			return current.GetVariable<object>(name,index,flags);
		}
		public static T GetVariable<T>(this object current,string name,int index=-1,BindingFlags flags=allFlags){
			if(current.IsNull()){return default(T);}
			if(name.IsNumber()){
				index = name.ToInt();
				name = "";
			}
			var value = default(T);
			object instance = current is Type || current.IsStatic() ? null : current;
			var type = current is Type ? (Type)current : current.GetType();
			var property = Class.GetProperty(type,name,flags);
			var field = Class.GetField(type,name,flags);
			if(!name.IsEmpty() && property.IsNull() && field.IsNull()){
				if(ObjectExtension.debug && !Class.warned.AddNew(current).AddNew(name)){
					Debug.LogWarning("[ObjectReflection] Could not find variable to get -- " + type.Name + "." + name);
					Class.warned[current][name] = true;
				}
				return value;
			}
			if(property != null){value = (T)property.GetValue(instance,null);}
			if(field != null){value = (T)field.GetValue(instance);}
			if(index != -1){
				if(name.IsEmpty()){
					if(current is IList){return current.As<IList<T>>()[index];}
					if(current is Vector3){return current.As<Vector3>()[index].As<T>();}
				}
				if(value is Vector3){return value.As<Vector3>()[index].As<T>();}
				return value.As<IList>()[index].As<T>();
			}
			return value;
		}
		public static void SetVariables<T>(this object current,IDictionary<string,T> values,BindingFlags flags=allFlags){
			foreach(var item in values){
				current.SetVariable<T>(item.Key,item.Value,-1,flags);
			}
		}
		public static void SetVariable<T>(this object current,string name,T value,int index=-1,BindingFlags flags=allFlags){
			if(current.IsNull()){return;}
			if(name.IsNumber()){
				index = name.ToInt();
				name = "";
			}
			var instance = current is Type || current.IsStatic() ? null : current;
			var type = current is Type ? (Type)current : current.GetType();
			var property = Class.GetProperty(type,name,flags);
			var field = Class.GetField(type,name,flags);
			if(!name.IsNull() && property.IsNull() && field.IsNull() && !Class.warned.AddNew(current).AddNew(name)){
				if(ObjectExtension.debug){Debug.LogWarning("[ObjectReflection] Could not find variable to set -- " + name);}
				Class.warned[current][name] = true;
				return;
			}
			if(index != -1){
				if(name.IsEmpty()){
					if(current is IList){current.As<IList<T>>()[index] = value;}
					if(current is Vector3){
						var goal = current.As<Vector3>();
						goal[index] = value.ToFloat();
						current.As<Vector3>().Set(goal.x,goal.y,goal.z);
					}
					return;
				}
				object existing = field.IsNull() ? property.GetValue(instance,null) : field.GetValue(instance);
				if(existing is Vector3){
					var goal = existing.As<Vector3>();
					goal[index] = value.ToFloat();
					existing.As<Vector3>().Set(goal.x,goal.y,goal.z);
					return;
				}
				existing.As<Array>().SetValue(value,index);
				return;
			}
			if(property != null && property.CanWrite){
				property.SetValue(instance,value,null);
			}
			if(field != null && !field.FieldType.IsGenericType){
				field.SetValue(instance,value);
			}
		}
		public static Dictionary<string,T> GetVariables<T>(this object current,IList<Type> withoutAttributes=null,BindingFlags flags=allFlags){
			var allVariables = current.GetVariables(withoutAttributes,flags);
			return allVariables.Where(x=>x.Value.Is<T>()).ToDictionary(x=>x.Key,x=>(T)x.Value);
		}
		public static Dictionary<string,object> GetVariables(this object current,IList<Type> withoutAttributes=null,BindingFlags flags=allFlags){
			if(current.IsNull()){return new Dictionary<string,object>();}
			Type type = current is Type ? (Type)current : current.GetType();
			var target = Class.variables.AddNew(type).AddNew(flags);
			IEnumerable<KeyValuePair<IList<Type>,Dictionary<string,object>>> match = null;
			if(withoutAttributes.IsNull()){
				withoutAttributes = Class.emptyList;
				if(target.ContainsKey(withoutAttributes)){
					return target[withoutAttributes];
				}
			}
			else{
				match = target.Where(x=>x.Key.SequenceEqual(withoutAttributes));
			}
			if(match.IsNull() || match.Count() < 1){
				object instance = current.IsStatic() || current is Type ? null : current;
				Dictionary<string,object> variables = new Dictionary<string,object>();
				foreach(FieldInfo field in type.GetFields(flags)){
					if(withoutAttributes.Count > 0){
						var attributes = Attribute.GetCustomAttributes(field);
						if(attributes.Any(x=>withoutAttributes.Any(y=>y==x.GetType()))){continue;}
					}
					variables[field.Name] = field.GetValue(instance);
				}
				foreach(PropertyInfo property in type.GetProperties(flags).Where(x=>x.CanRead)){
					if(!property.CanWrite){continue;}
					if(withoutAttributes.Count > 0){
						var attributes = Attribute.GetCustomAttributes(property);
						if(attributes.Any(x=>withoutAttributes.Any(y=>y==x.GetType()))){continue;}
					}
					try{variables[property.Name] = property.GetValue(instance,null);}
					catch{}
				}
				target[withoutAttributes] = variables;
				return variables;
			}
			return match.ToDictionary().Values.FirstOrDefault();
		}
		public static List<string> ListVariables(this object current,IList<Type> withoutAttributes=null,BindingFlags flags=allFlags){
			return current.GetVariables(withoutAttributes,flags).Keys.ToList();
		}
		public static void UseVariables<T>(this T current,T other,IList<Type> withoutAttributes=null,BindingFlags flags = publicFlags) where T : class{
			foreach(var name in current.ListVariables(withoutAttributes,flags)){
				current.SetVariable(name,other.GetVariable(name));
			}
		}
		public static void ClearVariable(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			current = current.IsStatic() ? null : current;
			FieldInfo field = Class.GetField(type,name,flags);
			if(!field.IsNull() && !field.FieldType.IsGenericType){
				field.SetValue(current,null);
				return;
			}
			PropertyInfo property = Class.GetProperty(type,name,flags);
			if(!property.IsNull() && property.CanWrite){
				property.SetValue(current,null,null);
			}
		}
		//=========================
		// Values
		//=========================
		public static void SetValuesByType<T>(this object current,IList<T> values,IList<Type> withoutAttributes=null,BindingFlags flags=allFlags){
			var existing = current.GetVariables<T>(withoutAttributes,flags);
			int index = 0;
			foreach(var item in existing){
				if(index >= values.Count){break;}
				current.SetVariable(item.Key,values[index]);
				++index;
			}
		}
		public static void SetValuesByName<T>(this object current,Dictionary<string,T> values,IList<Type> withoutAttributes=null,BindingFlags flags=allFlags){
			var existing = current.GetVariables<T>(withoutAttributes,flags);
			foreach(var item in existing){
				if(!values.ContainsKey(item.Key)){
					if(ObjectExtension.debug){Debug.Log("[ObjectReflection] : No key found when attempting to assign values by name -- " + item.Key);}
					continue;
				}
				current.SetVariable(item.Key,values[item.Key]);
			}
		}
		public static T[] GetValues<T>(this object current,IList<Type> withoutAttributes=null,BindingFlags flags=allFlags){
			var allVariables = current.GetVariables<T>(withoutAttributes,flags);
			return allVariables.Values.ToArray();
		}
	}
}