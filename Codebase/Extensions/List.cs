using System.Collections.Generic;
using System.Reflection;
using System;
public static class ListExtension{
	public static List<T> Copy<T>(this IEnumerable<T> current){
		return new List<T>(current);
	}
	public static void Move<T>(this List<T> current,int index,int newIndex) where T : class{
		T item = current[index];
		current.Remove(item);
		current.Insert(newIndex,item);
	}
	public static int IndexOf<T>(this List<T> current,T type){
		return Array.IndexOf(current.ToArray(),type);
	}
	public static int IndexOf<T>(this List<T> current,Enum enumerable){
		string name = enumerable.ToString();
		return current.ToArray().IndexOf(name);
	}
	public static List<T> Shuffle<T>(this IList<T> current){ 
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
	public static void Sort<T>(this List<T> current,SortOptions options){
		current.Sort(new Comparer<T>(options));
	}
	public static void Sort<T>(this List<T> current,SortOptions options,OnCompareEvent onCompare){
		current.Sort(new Comparer<T>(options,onCompare));
	}
}
public enum SortOrientation{
	Ascending,
	Descending}
;
public class SortOptions{
	public int fieldNumber = -1;
	public string field;
	public SortOrientation orientation;
	public void Setup(string field){
		if(this.field != null && this.field.Equals(field)){
			if(this.orientation == SortOrientation.Ascending){
				this.orientation = SortOrientation.Descending;
			}
			else{
				this.orientation = SortOrientation.Ascending;
			}
		}
		else{
			this.field = field;
			this.orientation = SortOrientation.Ascending;
		}
	}
	public void Setup(int fieldNumber){
		if(this.fieldNumber != -1 && this.fieldNumber == fieldNumber){
			if(this.orientation == SortOrientation.Ascending){
				this.orientation = SortOrientation.Descending;
			}
			else{
				this.orientation = SortOrientation.Ascending;
			}
		}
		else{
			this.fieldNumber = fieldNumber;
			this.orientation = SortOrientation.Ascending;
		}
	}
}
public delegate int OnCompareEvent(object target1,object target2);
class Comparer<T> : IComparer<T>{
	public string field;
	public SortOrientation orientation;
	public OnCompareEvent onCompare;
	public Comparer(SortOptions options,OnCompareEvent onCompare = null){
		this.field = options.field;
		this.orientation = options.orientation;
		this.onCompare = onCompare;
	}
	public int Compare(T element1,T element2){
		int returnValue;
		if(this.onCompare != null){
			returnValue = onCompare(element1,element2);
		}
		else{
			object firstValue = element1.GetType().GetField(this.field).GetValue(element1);
			object secondValue = element2.GetType().GetField(this.field).GetValue(element2);
			MethodInfo compareMethod = firstValue.GetType().GetMethod("CompareTo",new Type[] { typeof(object) });
			returnValue = (int)compareMethod.Invoke(firstValue,new object[] { secondValue });
		}
		if(this.orientation == SortOrientation.Descending){
			if(returnValue == 1){
				returnValue = -1;
			}
			else if(returnValue == -1){
				returnValue = 1;
			}
		}
		return returnValue;
	}
}