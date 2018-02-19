using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Zios.Extensions.Convert{
	using Zios.Extensions;
	public static class ConvertIEnumerable{
		public static To[] ConvertAll<To>(this IEnumerable<string> current){
			return current.ConvertAll<string,To>();
		}
		public static To[] ConvertAll<From,To>(this IEnumerable<From> current){
			var source = current.ToArray<From>();
			return Array.ConvertAll(source,x=>x.Convert<To>()).ToArray();
		}
		public static Dictionary<TKey,TValue> ToDictionary<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> current){
			return current.ToDictionary(x=>x.Key,x=>x.Value);
		}
		public static string ToText<T>(this IEnumerable<T> current){
			var value = new StringBuilder();
			foreach(var item in current){
				value.Append(item.ToString());
				value.Append(" | ");
			}
			return value.ToString().TrimRight(" | ");
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
		public static int[] ToInt(this IEnumerable<string> current){return current.Select(x=>x.ToInt()).ToArray();}
		public static bool[] ToBool(this IEnumerable<string> current){return current.Select(x=>x.ToBool()).ToArray();}
		public static float[] ToFloat(this IEnumerable<string> current){return current.Select(x=>x.ToFloat()).ToArray();}
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> current){
		   return new HashSet<T>(current);
		}
		public static string Serialize<T>(this IEnumerable<T> current){
			string output = "";
			foreach(var value in current){
				output += value.SerializeAuto()+"-";
			}
			return output.TrimRight("-");
		}
		public static IEnumerable<T> Deserialize<T>(this IEnumerable<T> current,string value){
			return value.Split("-").Select(x=>x.Deserialize<T>()).ToArray();
		}
	}
}