using System;
public static class TypeExtension{
	public static bool IsStatic(this Type current){
		return current.IsAbstract && current.IsSealed;
	}
	public static bool IsType(this Type current,Type value){
		return (current == value) || (current.IsSubclassOf(value));
	}
}