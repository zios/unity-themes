using System.Collections.Generic;
using System.Reflection;
using System;
public static class ListExtension{
	public static List<T> Copy<T>(this IEnumerable<T> current){
		return new List<T>(current);
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
}
public enum SortOrientation{
	Ascending,
	Descending
};
public class SortOptions{
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
}
class Comparer<T> : IComparer<T>{
	public string field;
	public SortOrientation orientation;
	public Comparer(SortOptions options){
		this.field = options.field;
		this.orientation = options.orientation;
	}
	public int Compare(T element1,T element2){
		object firstValue = element1.GetType().GetField(this.field).GetValue(element1);
		object secondValue = element2.GetType().GetField(this.field).GetValue(element2);
		MethodInfo compareMethod = firstValue.GetType().GetMethod("CompareTo",new Type[]{typeof(object)});
		int returnValue = (int)compareMethod.Invoke(firstValue,new object[]{secondValue});
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