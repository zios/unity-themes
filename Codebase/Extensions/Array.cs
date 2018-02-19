using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios.Extensions{
	public static class ArrayExtension{
		//=======================
		// Default
		//=======================
		public static int IndexOf<T>(this Array current,T value){
			return Array.IndexOf(current,value);
		}
		public static int IndexOf<T>(this Array current,Enum enumerable){
			string name = enumerable.ToString();
			return current.IndexOf(name);
		}
		public static T[] Copy<T>(this T[] current){
			T[] result = new T[current.Length];
			current.CopyTo(result,0);
			return result;
		}
		public static T[] Concat<T>(this T[] current,T[] list){
			T[] result = new T[current.Length + list.Length];
			current.CopyTo(result,0);
			list.CopyTo(result,current.Length);
			return result;
		}
		public static bool Exists<T>(this T[] current,Predicate<T> predicate){
			return Array.Exists(current,predicate);
		}
		public static T Find<T>(this T[] current,Predicate<T> predicate){
			return Array.Find(current,predicate);
		}
		public static T[] Clear<T>(this T[] current){
			return new T[0]{};
		}
		public static T[] Add<T>(this T[] current,T element){
			T[] extra = new T[]{element};
			return current.Concat(extra);
		}
		public static T[] Remove<T>(this T[] current,T value){
			List<T> copy = new List<T>(current);
			copy.Remove(value);
			return copy.ToArray();
		}
		public static T[] RemoveAt<T>(this T[] current,int index){
			List<T> copy = new List<T>(current);
			copy.RemoveAt(index);
			return copy.ToArray();
		}
		public static T[] RemoveAll<T>(this T[] current,T value){
			List<T> copy = new List<T>(current);
			copy.RemoveAll(x=>x.Equals(value));
			return copy.ToArray();
		}
		public static T[] Resize<T>(this T[] current,int newSize){
			while(current.Length > newSize){
				current = current.RemoveAt(current.Length-1);
			}
			while(current.Length < newSize){
				current = current.Add(default(T));
			}
			return current;
		}
		public static T[] Order<T>(this T[] current){
			var copy = current.Copy().ToList();
			copy.Sort();
			return copy.ToArray();
		}
		public static bool HasAny<T>(this T[] current,params T[] values){return current.ContainsAny(values);}
		public static bool HasAll<T>(this T[] current,params T[] values){return current.ContainsAll(values);}
		public static bool ContainsAny<T>(this T[] current,params T[] values){
			for(int index=0;index<values.Length;++index){
				if(current.Contains(values[index])){return true;}
			}
			return false;
		}
		public static bool ContainsAll<T>(this T[] current,params T[] values){
			for(int index=0;index<values.Length;++index){
				if(!current.Contains(values[index])){return false;}
			}
			return true;
		}
		public static bool ContainsAmount<T>(this T[] current,int amount,params T[] values){
			var count = 0;
			for(int index=0;index<values.Length;++index){
				if(current.Contains(values[index])){count += 1;}
			}
			return count >= amount;
		}
		public static void ForEach<T>(this T[] current,Action<T> method){
			for(int index=0;index<current.Length;++index){
				method(current[index]);
			}
		}
		//=======================
		// Float
		//=======================
		public static float[] Scale(this float[] current,float scalar){
			float[] result = current;
			for(int index=0;index<current.Length;++index){
				result[index] = current[index] * scalar;
			}
			return result;
		}
	}
}