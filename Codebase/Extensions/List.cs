using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
namespace Zios{
    public static class ListExtension{
	    public static List<T> Copy<T>(this List<T> current){
		    return new List<T>(current);
		}
	    public static void Move<T>(this List<T> current,int index,int newIndex) where T : class{
		    T item = current[index];
		    current.Remove(item);
		    current.Insert(newIndex,item);
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
		    System.Random random = new System.Random();  
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
	}
}
