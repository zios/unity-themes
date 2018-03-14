using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Reflection{
	using System.Linq.Expressions;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Supports.Hierarchy;
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	public static partial class Reflection{
		private static Dictionary<string,Type> internalTypes = new Dictionary<string,Type>();
		public static List<Assembly> assemblies = new List<Assembly>();
		public static Hierarchy<Type,BindingFlags,IList<Type>,string,object> variables = new Hierarchy<Type,BindingFlags,IList<Type>,string,object>();
		public static Hierarchy<Type,BindingFlags,string,PropertyInfo> properties = new Hierarchy<Type,BindingFlags,string,PropertyInfo>();
		public static Hierarchy<Type,BindingFlags,string,FieldInfo> fields = new Hierarchy<Type,BindingFlags,string,FieldInfo>();
		public static Hierarchy<Type,BindingFlags,List<MethodInfo>> methods = new Hierarchy<Type,BindingFlags,List<MethodInfo>>();
		//=========================
		// Interface
		//=========================
		public static List<Type> GetSubTypes<Scope>(){
			var assemblies = Reflection.GetAssemblies();
			var matches = new List<Type>();
			foreach(var assembly in assemblies){
				var types = assembly.GetTypes();
				foreach(var type in types){
					if(type.IsSubclassOf(typeof(Scope))){
						matches.Add(type);
					}
				}
			}
			return matches;
		}
		public static Type GetType(string path){
			var assemblies = Reflection.GetAssemblies();
			foreach(var assembly in assemblies){
				Type[] types = assembly.GetTypes();
				foreach(Type type in types){
					if(type.FullName == path){
						return type;
					}
				}
			}
			return null;
		}
		public static List<Assembly> GetAssemblies(){
			if(Reflection.assemblies.Count < 1){Reflection.assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();}
			return Reflection.assemblies;
		}
		//=========================
		// Unity
		//=========================
		public static Type GetUnityType(string name){
			#if UNITY_EDITOR
			if(Reflection.internalTypes.ContainsKey(name)){return Reflection.internalTypes[name];}
			var fullCheck = name.ContainsAny(".","+");
			var alternative = name.ReplaceLast(".","+");
			var term = alternative.Split("+").Last();
			foreach(var type in typeof(UnityEditor.Editor).Assembly.GetTypes()){
				bool match = fullCheck && (type.FullName.Contains(name) || type.FullName.Contains(alternative)) && term.Matches(type.Name,true);
				if(type.Name == name || match){
					Reflection.internalTypes[name] = type;
					return type;
				}
			}
			foreach(var type in typeof(UnityEngine.Object).Assembly.GetTypes()){
				bool match = fullCheck && (type.FullName.Contains(name) || type.FullName.Contains(alternative)) && term.Matches(type.Name,true);
				if(type.Name == name || match){
					Reflection.internalTypes[name] = type;
					return type;
				}
			}
			#endif
			return null;
		}
		//=========================
		// Internal
		//=========================
		public static void ResetCache(){
			Reflection.assemblies.Clear();
			Reflection.internalTypes.Clear();
			Reflection.properties.Clear();
			Reflection.variables.Clear();
			Reflection.methods.Clear();
			Reflection.fields.Clear();
		}
		public static List<MethodInfo> GetMethods(Type type,BindingFlags flags=allFlags){
			var target = Reflection.methods.AddNew(type).AddNew(flags);
			if(target.Count < 1){
				foreach(var method in type.GetMethods(flags)){
					target.Add(method);
				}
			}
			return target;
		}
		public static MethodInfo GetMethod(Type type,string name,BindingFlags flags=allFlags){
			var target = Reflection.methods.AddNew(type).AddNew(flags);
			var method = target.FirstOrDefault(x=>x.Name==name);
			if(method == null){
				method = type.GetMethod(name,flags);
				if(method != null){target.Add(method);}
			}
			return method;
		}
		public static PropertyInfo GetProperty(Type type,string name,BindingFlags flags=allFlags){
			var target = Reflection.properties.AddNew(type).AddNew(flags);
			var property = target.TryGet(name);
			if(property == null){target[name] = property = type.GetProperty(name,flags);}
			return property;
		}
		public static FieldInfo GetField(Type type,string name,BindingFlags flags=allFlags){
			var target = Reflection.fields.AddNew(type).AddNew(flags);
			var field = target.TryGet(name);
			if(field == null){target[name] = field = type.GetField(name,flags);}
			return field;
		}
	}
	public static partial class Reflection{
		public static bool debug;
		public static List<Type> emptyList = new List<Type>();
		public static Hierarchy<object,string,bool> debugWarned = new Hierarchy<object,string,bool>();
		public static Hierarchy<Assembly,string,Type> lookup = new Hierarchy<Assembly,string,Type>();
		public static Hierarchy<Type,BindingFlags,IList<Type>,string,object> attributedVariables = new Hierarchy<Type,BindingFlags,IList<Type>,string,object>();
		public static Hierarchy<Type,BindingFlags,IList<Type>,string,MethodInfo> exactMethods = new Hierarchy<Type,BindingFlags,IList<Type>,string,MethodInfo>();
		public const BindingFlags allFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		public const BindingFlags allFlatFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.FlattenHierarchy;
		public const BindingFlags allDeclaredFlags = BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly;
		public const BindingFlags staticFlags = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
		public const BindingFlags staticPublicFlags = BindingFlags.Static|BindingFlags.Public;
		public const BindingFlags declaredFlags = BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly;
		public const BindingFlags instanceFlags = BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		public const BindingFlags privateFlags = BindingFlags.Instance|BindingFlags.NonPublic;
		public const BindingFlags publicFlags = BindingFlags.Instance|BindingFlags.Public;
		//=========================
		// Class
		//=========================
		public static Type GetClass(this Type current,string name){
			return current.GetNestedTypes(Reflection.allFlags).FirstOrDefault(x=>x.Name==name);
		}
		//=========================
		// Generics
		//=========================
		public static Type[] GetGenerics(this object current){
			return current.AsType().GetGenericArguments();
		}
		//=========================
		// Method
		//=========================
		public static bool HasMethod(this object current,string name,BindingFlags flags=allFlags){
			return !Reflection.GetMethod(current.AsType(),name,flags).IsNull();
		}
		public static MethodInfo GetExactMethod(this object current,string name,BindingFlags flags=allFlags,params object[] parameters){
			var argumentTypes = new List<Type>();
			foreach(var parameter in parameters){
				var type = parameter == null ? typeof(object) : parameter.GetType();
				argumentTypes.Add(type);
			}
			return current.GetExactMethods(argumentTypes,name,flags).FirstOrDefault();
		}
		public static MethodInfo GetExactMethod(this object current,IList<Type> argumentTypes=null,string name="",BindingFlags flags=allFlags){
			return current.GetExactMethods(argumentTypes,name,flags).FirstOrDefault();
		}
		public static List<MethodInfo> GetExactMethods(this object current,IList<Type> argumentTypes=null,string name="",BindingFlags flags=allFlags){
			var type = current.AsType();
			var methods = Reflection.exactMethods.AddNew(type).AddNew(flags).AddNew(argumentTypes);
			if(name.IsEmpty() && methods.Count > 0){return methods.Select(x=>x.Value).ToList();}
			MethodInfo existing;
			if(methods.TryGetValue(name,out existing)){return existing.AsList();}
			foreach(var method in Reflection.GetMethods(type,flags)){
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
			var method = current.GetExactMethod(name,flags,parameters);
			if(method == null){
				if(Reflection.debug){Log.Warning("[Object] No method found to call -- " + name);}
				return default(V);
			}
			if(current.IsStatic() || current is Type){
				return (V)method.Invoke(null,parameters);
			}
			return (V)method.Invoke(current,parameters);
		}
		public static object CallPath(this string current,params object[] parameters){
			if(Reflection.lookup.Count < 1){
				foreach(var assembly in Reflection.GetAssemblies()){
					var types = assembly.GetTypes();
					foreach(var type in types){
						Reflection.lookup.AddNew(assembly)[type.FullName] = type;
					}
				}
			}
			var methodName = current.Split(".").Last();
			var path = current.Remove("."+methodName);
			foreach(var member in Reflection.lookup){
				Type existing;
				var assembly = member.Value;
				assembly.TryGetValue(path,out existing);
				if(existing != null){
					var method = Reflection.GetMethod(existing,methodName);
					if(method.IsNull()){
						if(Reflection.debug){Log.Show("[ObjectReflection] Cannot call. Method does not exist -- " + current + "()");}
						return null;
					}
					if(!method.IsStatic){
						if(Reflection.debug){Log.Show("[ObjectReflection] Cannot call. Method is not static -- " + current + "()");}
						return null;
					}
					var value = existing.CallExactMethod(methodName,parameters);
					return value ?? true;
				}
			}
			if(Reflection.debug){Log.Show("[ObjectReflection] Cannot call. Path not found -- " + current + "()");}
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
			var method = Reflection.GetMethod(current.AsType(),name,flags);
			if(method.IsNull()){
				if(Reflection.debug){Log.Warning("[Object] No method found to call -- " + name);}
				return default(V);
			}
			if(current.IsStatic() || current is Type){
				return (V)method.Invoke(null,parameters);
			}
			return (V)method.Invoke(current,parameters);
		}
		//=========================
		// Attribute
		//=========================
		public static Dictionary<string,object> GetAttributedVariables(this object current,IList<Type> withAttributes=null,BindingFlags flags=allFlags){
			if(current.IsNull()){return new Dictionary<string,object>();}
			var type = current.AsType();
			var target = Reflection.attributedVariables.AddNew(type).AddNew(flags);
			var existing = target.Where(x=>x.Key.SequenceEqual(withAttributes));
			if(existing.Count() > 0){return existing.FirstOrDefault().Value;}
			var matches = target.AddNew(withAttributes);
			var allVariables = current.GetVariables(flags);
			foreach(var variable in allVariables){
				var name = variable.Key;
				var value = variable.Value;
				var missing = false;
				foreach(var attribute in withAttributes){
					if(!current.HasAttribute(name,attribute) && Attribute.GetCustomAttribute(value.GetType(),attribute).IsNull()){
						missing = true;
						break;
					}
				}
				if(missing){continue;}
				matches[name] = value;
			}
			return matches;
		}
		public static bool HasAttribute(this object current,string name,Type attribute){
			return current.ListAttributes(name).Exists(x=>x.GetType()==attribute);
		}
		public static Attribute[] ListAttributes(this object current,string name){
			var type = current.AsType();
			var field = Reflection.GetField(type,name,allFlags);
			if(!field.IsNull()){return Attribute.GetCustomAttributes(field);}
			var property = Reflection.GetProperty(type,name,allFlags);
			return property.IsNull() ? new Attribute[0] : Attribute.GetCustomAttributes(property);
		}
		//=========================
		// Variable
		//=========================
		public static bool HasVariable(this object current,string name,BindingFlags flags=allFlags){
			var type = current.AsType();
			return !Reflection.GetField(type,name,flags).IsNull() || !Reflection.GetProperty(type,name,flags).IsNull();
		}
		public static Type GetVariableType(this object current,string name,int index=-1,BindingFlags flags=allFlags){
			var type = current.AsType();
			var field = Reflection.GetField(type,name,flags);
			if(!field.IsNull()){
				if(index != -1){
					if(current is Vector3){return typeof(float);}
					IList list = (IList)field.GetValue(current);
					return list[index].GetType();
				}
				return field.FieldType;
			}
			var property = Reflection.GetProperty(type,name,flags);
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
			var type = current.AsType();
			var field = Reflection.GetField(type,name,flags);
			var property = field.IsNull() ? Reflection.GetProperty(type,name,flags) : null;
			if(!name.IsEmpty() && property.IsNull() && field.IsNull()){
				if(Reflection.debug && !Reflection.debugWarned.AddNew(current).AddNew(name)){
					Log.Warning("[ObjectReflection] Could not find variable to get -- " + type.Name + "." + name);
					Reflection.debugWarned[current][name] = true;
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
			var type = current.AsType();
			var field = Reflection.GetField(type,name,flags);
			var property = field.IsNull() ? Reflection.GetProperty(type,name,flags) : null;
			if(!name.IsNull() && property.IsNull() && field.IsNull() && !Reflection.debugWarned.AddNew(current).AddNew(name)){
				if(Reflection.debug){Log.Warning("[ObjectReflection] Could not find variable to set -- " + name);}
				Reflection.debugWarned[current][name] = true;
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
		public static Dictionary<string,T> GetVariables<T>(this object current,BindingFlags flags=allFlags){
			return current.GetVariables<T>(null,flags);
		}
		public static Dictionary<string,object> GetVariables(this object current,BindingFlags flags=allFlags){
			return current.GetVariables(null,flags);
		}
		public static Dictionary<string,T> GetVariables<T>(this object current,IList<Type> withoutAttributes,BindingFlags flags=allFlags){
			var allVariables = current.GetVariables(withoutAttributes,flags);
			return allVariables.Where(x=>x.Value.Is<T>()).ToDictionary(x=>x.Key,x=>(T)x.Value);
		}
		public static Dictionary<string,object> GetVariables(this object current,IList<Type> withoutAttributes,BindingFlags flags=allFlags){
			if(current.IsNull()){return new Dictionary<string,object>();}
			Type type = current.AsType();
			var target = Reflection.variables.AddNew(type).AddNew(flags);
			IEnumerable<KeyValuePair<IList<Type>,Dictionary<string,object>>> match = null;
			if(withoutAttributes.IsNull()){
				withoutAttributes = Reflection.emptyList;
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
			var type = current.AsType();
			current = current.IsStatic() ? null : current;
			var field = Reflection.GetField(type,name,flags);
			if(!field.IsNull()){
				if(!field.FieldType.IsGeneric()){
					field.SetValue(current,null);
				}
				return;
			}
			var property = Reflection.GetProperty(type,name,flags);
			if(!property.IsNull() && property.CanWrite){
				//var emitter = Emit<Func<object,string>>.NewDynamicMethod("Clear"+name).LoadArgument(3).CastClass(type).Call(property.GetGetMethod()).Return();
				//var setter = emitter.CreateDelegate();
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
		// Value
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
					if(Reflection.debug){Log.Show("[ObjectReflection] : No key found when attempting to assign values by name -- " + item.Key);}
					continue;
				}
				current.SetVariable(item.Key,values[item.Key]);
			}
		}
		public static T[] GetValues<T>(this object current,IList<Type> withoutAttributes=null,BindingFlags flags=allFlags){
			var allVariables = current.GetVariables<T>(withoutAttributes,flags);
			return allVariables.Values.ToArray();
		}
		//=========================
		// Misc
		//=========================
		public static string GetAlias(this object current){
			if(current.HasVariable("alias")){return current.GetVariable<string>("alias");}
			//if(current.HasVariable("name")){return current.GetVariable<string>("name");}
			return current.GetType().Name;
		}
		public static T Copy<T>(this T target) where T : class,new(){
			return new T().UseVariables(target);
		}
		public static T Clone<T>(this T target) where T : class{
			if(target.IsNull()){return null;}
			MethodInfo method = target.GetType().GetMethod("MemberwiseClone",Reflection.privateFlags);
			if(method != null){
				return (T)method.Invoke(target,null);
			}
			return null;
		}
	}
	public static class UnityObjectExtensions{
		public static bool IsExpanded(this UnityObject current){
			Type editorUtility = Reflection.GetUnityType("InternalEditorUtility");
			return editorUtility.Call<bool>("GetIsInspectorExpanded",current);
		}
		public static void SetExpanded(this UnityObject current,bool state){
			Type editorUtility = Reflection.GetUnityType("InternalEditorUtility");
			editorUtility.Call("SetIsInspectorExpanded",current,state);
		}
		public static void Destroy(this UnityObject target,bool destroyAssets=false){
			if(target.IsNull()){return;}
			if(target is Component){
				var component = target.As<Component>();
				if(component.gameObject.IsNull()){return;}
			}
			if(!Proxy.IsPlaying()){UnityObject.DestroyImmediate(target,destroyAssets);}
			else{UnityObject.Destroy(target);}
		}
	}
}