using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios.Extensions{
	public static class IEnumerableExtensions{
		//=======================
		// General
		//=======================
		public static bool ContainsAll<T>(this IEnumerable<T> current,IEnumerable<T> other){
			return !other.Except(current).Any();
		}
		public static IEnumerable<T> Diff<T>(this IEnumerable<T> current,IEnumerable<T> other){
			return current.Except(other).Concat(other.Except(current));
		}
		public static IEnumerable<T> Unshift<T>(this IEnumerable<T> current,T item){
			var result = current.ToList();
			result.Insert(0,item);
			return result;
		}
		public static IEnumerable<T> ReverseOrder<T>(this IEnumerable<T> current){
			current.Reverse();
			return current;
		}
		//=======================
		// LINQ-ish
		//=======================
		public static void Map<From,To>(this IEnumerable<From> current,IList<To> other,Action<To,From> method){
			var index = 0;
			foreach(var value in current){
				if(index >= other.Count()){break;}
				method(other[index],value);
				index += 1;
			}
		}
		public static List<Type> If<Type>(this IEnumerable<Type> current,Func<Type,bool> comparer){
			var results = new List<Type>();
			foreach(var item in current){
				if(comparer(item)){
					results.Add(item);
				}
			}
			return results;
		}
		public static IEnumerable<Type> SkipLast<Type>(this IEnumerable<Type> current){
			return current.SkipRight(1);
		}
		public static IEnumerable<Type> SkipRight<Type>(this IEnumerable<Type> current,int amount){
			return current.Take(current.Count() - amount);
		}
		public static IEnumerable<Type> TakeRight<Type>(this IEnumerable<Type> current,int amount){
			return current.Skip(current.Count() - amount).Take(amount);
		}
		//=======================
		// String
		//=======================
		public static string Join(this IEnumerable<string> current,string separator=" "){
			return string.Join(separator,current.ToArray());
		}
		public static List<string> Filter(this IEnumerable<string> current,string text){
			var newList = new List<string>();
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
			var results = new List<string>();
			foreach(string item in current){
				results.Add(item.Replace(replace,with));
			}
			return results;
		}
		public static List<string> AddSuffix(this IEnumerable<string> current,string suffix){
			var results = new List<string>();
			foreach(string item in current){
				results.Add(item+suffix);
			}
			return results;
		}
		public static string[] Trim(this IEnumerable<string> current,string values){return current.Select(x=>x.Trim(values)).ToArray();}
		public static string[] ToTitleCase(this IEnumerable<string> current){return current.Select(x=>x.ToTitleCase()).ToArray();}
		public static string[] ToCamelCase(this IEnumerable<string> current){return current.Select(x=>x.ToCamelCase()).ToArray();}
		public static string[] ToPascalCase(this IEnumerable<string> current){return current.Select(x=>x.ToPascalCase()).ToArray();}
	}
}
