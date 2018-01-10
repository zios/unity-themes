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
		public static bool debug;
		public static List<Type> emptyList = new List<Type>();
		public static List<Assembly> assemblies = new List<Assembly>();
		public static Hierarchy<Assembly,string,Type> lookup = new Hierarchy<Assembly,string,Type>();
		public static Hierarchy<object,string,bool> warned = new Hierarchy<object,string,bool>();
		public static Hierarchy<Type,BindingFlags,IList<Type>,string,object> attributedVariables = new Hierarchy<Type,BindingFlags,IList<Type>,string,object>();
		public static Hierarchy<Type,BindingFlags,IList<Type>,string,object> variables = new Hierarchy<Type,BindingFlags,IList<Type>,string,object>();
		public static Hierarchy<Type,BindingFlags,string,PropertyInfo> properties = new Hierarchy<Type,BindingFlags,string,PropertyInfo>();
		public static Hierarchy<Type,BindingFlags,string,FieldInfo> fields = new Hierarchy<Type,BindingFlags,string,FieldInfo>();
		public static Hierarchy<Type,BindingFlags,string,MethodInfo> methods = new Hierarchy<Type,BindingFlags,string,MethodInfo>();
		public static Hierarchy<Type,BindingFlags,IList<Type>,string,MethodInfo> exactMethods = new Hierarchy<Type,BindingFlags,IList<Type>,string,MethodInfo>();
		public const BindingFlags allFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		public const BindingFlags allFlatFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.FlattenHierarchy;
		public const BindingFlags staticFlags = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
		public const BindingFlags staticPublicFlags = BindingFlags.Static|BindingFlags.Public;
		public const BindingFlags instanceFlags = BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		public const BindingFlags privateFlags = BindingFlags.Instance|BindingFlags.NonPublic;
		public const BindingFlags publicFlags = BindingFlags.Instance|BindingFlags.Public;
		//=========================
		// Assemblies
		//=========================
		public static List<Assembly> GetAssemblies(){
			if(Class.assemblies.Count < 1){Class.assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();}
			return Class.assemblies;
		}
		//=========================
		// Methods
		//=========================
		public static List<MethodInfo> GetMethods(Type type,BindingFlags flags=allFlags){
			var target = Class.methods.AddNew(type).AddNew(flags);
			if(target.Count < 1){
				foreach(var method in type.GetMethods(flags)){
					target[method.Name] = method;
				}
			}
			return target.Values.ToList();
		}
		public static MethodInfo GetMethod(Type type,string name,BindingFlags flags=allFlags){
			var target = Class.methods.AddNew(type).AddNew(flags);
			MethodInfo method;
			if(!target.TryGetValue(name,out method)){target[name] = method = type.GetMethod(name,flags);}
			return method;
		}
		public static bool HasMethod(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			return !Class.GetMethod(type,name,flags).IsNull();
		}
		public static List<MethodInfo> GetExactMethods(this object current,IList<Type> argumentTypes=null,string name="",BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			var methods = Class.exactMethods.AddNew(type).AddNew(flags).AddNew(argumentTypes);
			if(name.IsEmpty() && methods.Count > 0){return methods.Select(x=>x.Value).ToList();}
			MethodInfo existing;
			if(methods.TryGetValue(name,out existing)){return existing.AsList();}
			foreach(var method in Class.GetMethods(type,flags)){
				if(!name.IsEmpty() && !method.Name.Matches(name,true)){continue;}
				if(!argumentTypes.IsNull()){
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
				methods[method.Name] = method;
			}
			return methods.Values.ToList();
		}
		public static List<string> ListMethods(this object current,IList<Type> argumentTypes=null,BindingFlags flags=allFlags){
			return current.GetExactMethods(argumentTypes,"",flags).Select(x=>x.Name).ToList();
		}
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
			var methods = current.GetExactMethods(argumentTypes,name,flags);
			if(methods.Count < 1){
				if(ObjectExtension.debug){Debug.LogWarning("[Object] No method found to call -- " + name);}
				return default(V);
			}
			if(current.IsStatic() || current is Type){
				return (V)methods[0].Invoke(null,parameters);
			}
			return (V)methods[0].Invoke(current,parameters);
		}
		public static object CallPath(this string current,params object[] parameters){
			if(Class.lookup.Count < 1){
				foreach(var assembly in Class.GetAssemblies()){
					var types = assembly.GetTypes();
					foreach(var type in types){
						Class.lookup.AddNew(assembly)[type.FullName] = type;
					}
				}
			}
			var methodName = current.Split(".").Last();
			var path = current.Remove("."+methodName);
			foreach(var member in Class.lookup){
				Type existing;
				var assembly = member.Value;
				assembly.TryGetValue(path,out existing);
				if(existing != null){
					var method = Class.GetMethod(existing,methodName);
					if(method.IsNull()){
						if(ObjectExtension.debug){Debug.Log("[ObjectReflection] Cannot call. Method does not exist -- " + current + "()");}
						return null;
					}
					if(!method.IsStatic){
						if(ObjectExtension.debug){Debug.Log("[ObjectReflection] Cannot call. Method is not static -- " + current + "()");}
						return null;
					}
					var value = existing.CallExactMethod(methodName,parameters);
					return value ?? true;
				}
			}
			if(ObjectExtension.debug){Debug.Log("[ObjectReflection] Cannot call. Path not found -- " + current + "()");}
			return null;
		}
		public static object Call(this object current,string name,params object[] parameters){
			return current.CallMethod<object>(name,allFlags,parameters);
		}
		public static V Call<V>(this object current,string name,params object[] parameters){
			return current.CallMethod<V>(name,allFlags,parameters);
		}
		public static object CallMethod(this object current,string name,params object[] parameters){
			return current.CallMethod<object>(name,allFlags,parameters);
		}
		public static V CallMethod<V>(this object current,string name,params object[] parameters){
			return current.CallMethod<V>(name,allFlags,parameters);
		}
		public static V CallMethod<V>(this object current,string name,BindingFlags flags,params object[] parameters){
			Type type = current is Type ? (Type)current : current.GetType();
			var method = Class.GetMethod(type,name,flags);
			if(method.IsNull()){
				if(ObjectExtension.debug){Debug.LogWarning("[Object] No method found to call -- " + name);}
				return default(V);
			}
			if(current.IsStatic() || current is Type){
				return (V)method.Invoke(null,parameters);
			}
			return (V)method.Invoke(current,parameters);
		}
		//=========================
		// Attributes
		//=========================
		public static bool HasAttribute(this object current,string name,Type attribute){
			return current.ListAttributes(name).Exists(x=>x.GetType()==attribute);
		}
		public static Attribute[] ListAttributes(this object current,string name){
			Type type = current is Type ? (Type)current : current.GetType();
			var field = Class.GetField(type,name,allFlags);
			if(!field.IsNull()){return Attribute.GetCustomAttributes(field);}
			var property = Class.GetProperty(type,name,allFlags);
			return property.IsNull() ? new Attribute[0] : Attribute.GetCustomAttributes(property);
		}
		public static Dictionary<string,object> GetAttributedVariables(this object current,IList<Type> withAttributes=null,BindingFlags flags=allFlags){
			if(current.IsNull()){return new Dictionary<string,object>();}
			Type type = current is Type ? (Type)current : current.GetType();
			var target = Class.attributedVariables.AddNew(type).AddNew(flags);
			var existing = target.Where(x=>x.Key.SequenceEqual(withAttributes)).FirstOrDefault();
			if(!existing.IsNull()){return existing.Value;}
			var matches = target.AddNew(withAttributes);
			var allVariables = current.GetVariables(null,flags);
			foreach(var variable in allVariables){
				var name = variable.Key;
				var value = variable.Value;
				foreach(var attribute in withAttributes){
					if(!value.HasAttribute(name,attribute)){continue;}
				}
				matches[name] = value;
			}
			return matches;
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
			PropertyInfo property;
			if(!target.TryGetValue(name,out property)){target[name] = property = type.GetProperty(name,flags);}
			return property;
		}
		public static FieldInfo GetField(Type type,string name,BindingFlags flags=allFlags){
			var target = Class.fields.AddNew(type).AddNew(flags);
			FieldInfo field;
			if(!target.TryGetValue(name,out field)){target[name] = field = type.GetField(name,flags);}
			return field;
		}
		public static bool HasVariable(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			return !Class.GetField(type,name,flags).IsNull() || !Class.GetProperty(type,name,flags).IsNull();
		}
		public static Type GetVariableType(this object current,string name,int index=-1,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			var field = Class.GetField(type,name,flags);
			if(!field.IsNull()){
				if(index != -1){
					if(current is Vector3){return typeof(float);}
					IList list = (IList)field.GetValue(current);
					return list[index].GetType();
				}
				return field.FieldType;
			}
			var property = Class.GetProperty(type,name,flags);
			return property.IsNull() ? typeof(Type) : property.PropertyType;
		}
		public static object GetVariable(this object current,string name,int index=-1,BindingFlags flags=allFlags){return current.GetVariable<object>(name,index,flags);}
		public static T GetVariable<T>(this object current,string name,BindingFlags flags){return current.GetVariable<T>(name,-1,flags);}
		public static T GetVariable<T>(this object current,string name,int index=-1,BindingFlags flags=allFlags){
			if(current.IsNull()){return default(T);}
			if(name.IsNumber()){
				index = name.ToInt();
				name = "";
			}
			if(current is IDictionary && current.As<IDictionary>().ContainsKey(name,true)){return current.As<IDictionary>()[name].As<T>();}
			if(index != -1 && current is Vector3){return current.As<Vector3>()[index].As<T>();}
			if(index != -1 && current is IList){return current.As<IList>()[index].As<T>();}
			var value = default(T);
			object instance = current is Type || current.IsStatic() ? null : current;
			var type = current is Type ? (Type)current : current.GetType();
			var field = Class.GetField(type,name,flags);
			var property = field.IsNull() ? Class.GetProperty(type,name,flags) : null;
			if(!name.IsEmpty() && property.IsNull() && field.IsNull()){
				if(ObjectExtension.debug && !Class.warned.AddNew(current).AddNew(name)){
					Debug.LogWarning("[ObjectReflection] Could not find variable to get -- " + type.Name + "." + name);
					Class.warned[current][name] = true;
				}
				return value;
			}
			if(!property.IsNull()){value = (T)property.GetValue(instance,null);}
			if(!field.IsNull()){value = (T)field.GetValue(instance);}
			if(index != -1 && (value is IList || value is Vector3)){
				return value.GetVariable<T>(name,index,flags);
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
			if(current is IDictionary){
				current.As<IDictionary>()[name] = value;
				return;
			}
			if(index != -1 && current is IList){
				current.As<IList>()[index] = value;
				return;
			}
			if(index != -1 && current is Vector3){
				var goal = current.As<Vector3>();
				goal[index] = value.ToFloat();
				current.As<Vector3>().Set(goal.x,goal.y,goal.z);
				return;
			}
			var instance = current is Type || current.IsStatic() ? null : current;
			var type = current is Type ? (Type)current : current.GetType();
			var field = Class.GetField(type,name,flags);
			var property = field.IsNull() ? Class.GetProperty(type,name,flags) : null;
			if(!name.IsNull() && property.IsNull() && field.IsNull() && !Class.warned.AddNew(current).AddNew(name)){
				if(ObjectExtension.debug){Debug.LogWarning("[ObjectReflection] Could not find variable to set -- " + name);}
				Class.warned[current][name] = true;
				return;
			}
			if(index != -1){
				var targetValue = property.IsNull() ? field.GetValue(instance) : property.GetValue(instance,null);
				if(targetValue is IList || targetValue is Vector3){
					targetValue.SetVariable<T>(name,value,index,flags);
					return;
				}
			}
			if(!field.IsNull() && !field.FieldType.IsGeneric()){
				field.SetValue(instance,value);
			}
			if(!property.IsNull() && property.CanWrite){
				property.SetValue(instance,value,null);
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
				Dictionary<string,object> existing;
				if(target.TryGetValue(withoutAttributes,out existing)){
					return existing;
				}
			}
			else{
				match = target.Where(x=>x.Key.SequenceEqual(withoutAttributes));
			}
			if(match.IsNull() || match.Count() < 1){
				object instance = current.IsStatic() || current is Type ? null : current;
				Dictionary<string,object> variables = new Dictionary<string,object>();
				foreach(FieldInfo field in type.GetFields(flags)){
					if(field.FieldType.IsGeneric()){continue;}
					if(withoutAttributes.Count > 0){
						var attributes = Attribute.GetCustomAttributes(field);
						if(attributes.Any(x=>withoutAttributes.Any(y=>y==x.GetType()))){continue;}
					}
					variables[field.Name] = field.GetValue(instance);
				}
				foreach(PropertyInfo property in type.GetProperties(flags).Where(x=>x.CanRead)){
					if(!property.CanWrite || property.PropertyType.IsGeneric()){continue;}
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
		public static T UseVariables<T>(this T current,T other,IList<Type> withoutAttributes=null,BindingFlags flags = publicFlags) where T : class{
			foreach(var name in current.ListVariables(withoutAttributes,flags)){
				current.SetVariable(name,other.GetVariable(name));
			}
			return current;
		}
		public static void ClearVariable(this object current,string name,BindingFlags flags=allFlags){
			Type type = current is Type ? (Type)current : current.GetType();
			current = current.IsStatic() ? null : current;
			FieldInfo field = Class.GetField(type,name,flags);
			if(!field.IsNull()){
				if(!field.FieldType.IsGeneric()){
					field.SetValue(current,null);
				}
				return;
			}
			PropertyInfo property = Class.GetProperty(type,name,flags);
			if(!property.IsNull() && property.CanWrite){
				property.SetValue(current,null,null);
			}
		}
		public static object InstanceVariable(this object current,string name,bool force){return current.InstanceVariable(name,-1,allFlags,force);}
		public static object InstanceVariable(this object current,string name,BindingFlags flags,bool force=false){return current.InstanceVariable(name,-1,flags,force);}
		public static object InstanceVariable(this object current,string name,int index=-1,BindingFlags flags=allFlags,bool force=false){
			object instance = current.GetVariable(name,index,flags);
			if(force || instance.IsNull()){
				var instanceType = current.GetVariableType(name,index,flags);
				if(!instanceType.GetConstructor(Type.EmptyTypes).IsNull()){
					try{
						instance = Activator.CreateInstance(instanceType);
						current.SetVariable(name,instance,index,flags);
					}
					catch{}
				}
			}
			return instance;
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