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
		    var copy = current.Copy();
		    copy.Sort();
		    return copy;
	    }
	    public static void Sort<T>(this List<T> current,SortOptions options){
		    current.Sort(new Comparer<T>(options));
	    }
	    public static void Sort<T>(this List<T> current,SortOptions options,OnCompareEvent onCompare){
		    current.Sort(new Comparer<T>(options,onCompare));
	    }
	    public static List<T> Extend<T>(this List<T> current,List<T> values){
		    List<T> copy = new List<T>(current);
		    copy.AddRange(values);
		    return copy;
	    }
    }
    public enum SortOrientation{
	    Ascending,
	    Descending
    };
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
}