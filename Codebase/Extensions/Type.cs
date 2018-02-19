using System;
using System.Collections;
namespace Zios.Extensions{
	public static class TypeExtension{
		public static bool Has(this Type current,object value){return current.Has(value.GetType());}
		public static bool Has(this Type current,Type value){
			if(value.IsInterface){return current.GetInterface(value.Name) != null;}
			return current.IsSubclassOf(typeof(Type));
		}
		public static bool HasEmptyConstructor(this Type current){
			return typeof(Type).GetConstructor(Type.EmptyTypes) != null;
		}
		public static bool IsCollection(this Type current){
			return current.Has(typeof(ICollection));
		}
		public static bool IsStatic(this Type current){
			return current.IsAbstract && current.IsSealed;
		}
		public static bool IsSubclass(this Type current,Type value){
			while(value != null && value != typeof(object)){
				var core = value.IsGenericType ? value.GetGenericTypeDefinition() : value;
				if(current == core){return true;}
				value = value.BaseType;
			}
			return false;
		}
	}
}