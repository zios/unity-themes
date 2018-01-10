using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
namespace Zios{
	public static class SerializedPropertyExtension{
		public static Dictionary<SerializedProperty,object> cache = new Dictionary<SerializedProperty,object>();
		public static object GetObject(this SerializedProperty current,bool parent=false){
			return current.GetObject<object>(parent);
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
		public static T GetObject<T>(this SerializedProperty current,bool parent=false){
			if(cache.ContainsKey(current)){return (T)cache[current];}
			object container = current.serializedObject.targetObject;
			string path = current.propertyPath.Replace(".Array.data[","[");
			string[] elements = path.Split('.');
			if(parent){elements = elements.Take(elements.Length-1).ToArray();}
			foreach(string element in elements){
				if(element.Contains("[")){
					string elementName = element.Substring(0,element.IndexOf("["));
					int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[","").Replace("]",""));
					container = container.GetVariable(elementName,index);
				}
				else{
					container = container.GetVariable(element);
				}
			}
			cache[current] = container;
			return (T)container;
		}
	}
}