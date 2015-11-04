using System;
namespace Zios{
	public static class TypeExtension{
		public static bool IsStatic(this Type current){
			return current.IsAbstract && current.IsSealed;
		}
		public static bool IsType(this Type current,Type value){
			return (current == value) || (current.IsSubclassOf(value));
		}
		public static bool IsSubclass(this Type current,Type value) {
			while(value != null && value != typeof(object)){
				var core = value.IsGenericType ? value.GetGenericTypeDefinition() : value;
				if(current == core){return true;}
				value = value.BaseType;
			}
			return false;
		}
	}
}