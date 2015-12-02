using System.Collections.Generic;
using System.Linq;
namespace Zios{
	public static class IEnumerableExtension{
		//=======================
		// General
		//=======================
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
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> current){
		   return new HashSet<T>(current);
		}
		//=======================
		// String
		//=======================
		public static string Join(this IEnumerable<string> current,string separator=" "){
			return string.Join(separator,current.ToArray());
		}
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
		public static List<string> Replace(this IEnumerable<string> current,string replace,string with,bool ignoreCase=true){
			List<string> results = new List<string>();
			foreach(string item in current){
				results.Add(item.Replace(replace,with));
			}
			return results;
		}
		public static string[] AddSuffix(this string[] current,string suffix){
			for(int index=0;index<current.Length;++index){
				current[index] += suffix;
			}
			return current;
		}
	}
}