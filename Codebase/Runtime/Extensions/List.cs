using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios.Extensions{
	public static class ListExtensions{
		public static List<T> Copy<T>(this List<T> current){
			return new List<T>(current);
		}
		public static void Move<T>(this List<T> current,int index,int newIndex) where T : class{
			T item = current[index];
			current.Remove(item);
			current.Insert(newIndex,item);
		}
		public static List<T> Unshift<T>(this List<T> current,T element){
			current.Insert(0,element);
			return current;
		}
		public static T Find<T>(this List<T> current,T value){
			return current.Find(x=>x.Equals(value));
		}
		public static bool Exists<T>(this List<T> current,T value){
			return current.Exists(x=>x.Equals(value));
		}
		public static bool Has<T>(this List<T> current,T value){
			foreach(T item in current){
				if(item.Equals(value)){return true;}
			}
			return false;
		}
		public static T AddNew<T>(this List<T> current) where T : new(){
			T item = new T();
			current.Add(item);
			return item;
		}
		public static T AddNew<T>(this List<T> current,T value){
			if(!current.Contains(value)){
				current.Add(value);
			}
			return value;
		}
		public static int IndexOf<T>(this List<T> current,T item){
			return current.FindIndex(x=>x.Equals(item));
		}
		public static int IndexOf<T>(this List<T> current,Enum enumerable){
			string name = enumerable.ToString();
			return current.ToArray().IndexOf(name);
		}
		public static List<T> Shuffle<T>(this List<T> current){
			List<T> copy = current.Copy();
			Random random = new Random();
			int total = copy.Count;
			while(total > 1){
				total--;
				int index = random.Next(total + 1);
				T value = copy[index];
				copy[index] = copy[total];
				copy[total] = value;
			}
			return copy;
		}
		public static List<string> ToLower(this List<string> current){
			List<string> newList = new List<string>();
			foreach(string item in current){
				newList.Add(item.ToLower());
			}
			return newList;
		}
		public static List<T> Order<T>(this List<T> current){
			//var copy = current.Copy();
			current.Sort();
			return current;
		}
		public static List<T> Extend<T>(this List<T> current,List<T> values){
			List<T> copy = new List<T>(current);
			copy.AddRange(values);
			return copy;
		}
		public static List<T> Delete<T>(this List<T> current,T value){
			current.Remove(value);
			return current;
		}
		public static List<T> Append<T>(this List<T> current,T value){
			current.Add(value);
			return current;
		}
		public static T TakeRandom<T>(this List<T> current){
			return current.Shuffle().First();
		}
		public static List<int> DivideSize<T>(this List<T> current,int size){
			var data = new int[size].ToList();
			var index = 0;
			while(index < current.Count){
				data[index%size] += 1;
				index += 1;
			}
			return data;
		}
		public static List<List<T>> DivideEvery<T>(this List<T> current,int amount){
			return current.DivideInto(current.Count / amount);
		}
		public static List<List<T>> DivideInto<T>(this List<T> current,int amount){
			var data = new List<List<T>>();
			var sizes = current.DivideSize(amount);
			var index = 0;
			foreach(var size in sizes){
				var count = 0;
				var entry = data.AddNew();
				while(count < size){
					entry.Add(current[count+index]);
					count += 1;
				}
				index += size;
			}
			return data;
		}
	}
}
