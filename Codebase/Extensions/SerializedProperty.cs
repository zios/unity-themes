using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
public static class SerializedPropertyExtension{
	public static object GetObject(this SerializedProperty current){
		return current.GetObject<object>();
	}
	static public T GetObject<T>(this SerializedProperty current){
		object parent = current.serializedObject.targetObject;
		string path = current.propertyPath.Replace(".Array.data[","[");
		string[] elements = path.Split('.');
		foreach(string element in elements){
			if(element.Contains("[")){
				var elementName = element.Substring(0,element.IndexOf("["));
				var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[","").Replace("]",""));
				parent = parent.GetAttribute(elementName,index);
			}
			else{
				parent = parent.GetAttribute(element);
			}
		}
		return (T)parent;
	}
}