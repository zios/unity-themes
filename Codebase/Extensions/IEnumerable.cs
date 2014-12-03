using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;	
public static class IEnumerableExtension{
	public static List<string> Filter(this IEnumerable<string> current,string text){
		List<string> newList = new List<string>();
		bool wildcard = text.Contains("*");
		text = text.Replace("*","");
		foreach(string item in current){
			if(wildcard && item.Contains(text)){
				newList.Add(item);
			}
			else if(item == text){
				newList.Add(item);
			}
		}
		return newList;
	}
	public static string ToString<T>(this IEnumerable<T> current,string separator=" ",string endTerm="or"){
		string result = "";
		foreach(var item in current){
			bool isLast = current.Last().Equals(item);
			if(isLast){result += endTerm;}
			result += item.ToString();
			if(!isLast){result += separator;}
		}
		return result;
	}
	public static List<string> Replace(this IEnumerable<string> current,string replace,string with,bool ignoreCase=true){
		List<string> results = new List<string>();
		foreach(string item in current){
			results.Add(item.Replace(replace,with));
		}
		return results;
	}
}