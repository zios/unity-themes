using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
namespace Zios{
	public static class SerializedPropertyExtension{
		public static object GetObject(this SerializedProperty current){
			return current.GetObject<object>();
		}
		public static int GetIndex(this SerializedProperty current){
			int index = -1;
			string path = current.propertyPath;
			if(path.EndsWith("]")){
				int start = path.LastIndexOf('[')+1;
				int end = path.IndexOf(']',start)-1;
				index = path.Cut(start,end).ToInt();
			}
			return index;
		}
		public static T GetObject<T>(this SerializedProperty current){
			object parent = current.serializedObject.targetObject;
			string path = current.propertyPath.Replace(".Array.data[","[");
			string[] elements = path.Split('.');
			foreach(string element in elements){
				if(element.Contains("[")){
					string elementName = element.Substring(0,element.IndexOf("["));
					int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[","").Replace("]",""));
					parent = parent.GetVariable(elementName,index);
				}
				else{
					parent = parent.GetVariable(element);
				}
			}
			return (T)parent;
		}
	}
}