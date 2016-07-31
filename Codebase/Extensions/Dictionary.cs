using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	public static class DictionaryExtension{
		public static SortedList<TKey,TValue> ToSortedList<TKey,TValue>(this Dictionary<TKey,TValue> current){
			return new SortedList<TKey,TValue>(current);
		}
		public static Dictionary<TKey,TValue> Copy<TKey,TValue>(this Dictionary<TKey,TValue> current){
			return new Dictionary<TKey,TValue>(current);
		}
		public static TValue Get<TKey,TValue>(this IDictionary<TKey,TValue> current,TKey key,TValue value=default(TValue)) where TValue : new(){
			if(!current.ContainsKey(key)){
				return value;
			}
			return current[key];
		}
		public static void SetValues<TKey,TValue>(this IDictionary<TKey,TValue> current,IList<TValue> values) where TValue : new(){
			int index = 0;
			foreach(var key in current.Keys.ToList()){
				current[key] = values[index];
				++index;
			}
		}
		public static TValue AddDefault<TKey,TValue>(this IDictionary<TKey,TValue> current,TKey key){
			if(!current.ContainsKey(key)){
				current[key] = default(TValue);
			}
			return current[key];
		}
		public static TValue AddNew<TKey,TValue>(this IDictionary<TKey,TValue> current,TKey key) where TValue : new(){
			if(!current.ContainsKey(key)){
				current[key] = new TValue();
			}
			return current[key];
		}
		public static TValue AddNewSequence<TKey,TValue>(this IDictionary<IList<TKey>,TValue> current,IList<TKey> key) where TValue : new(){
			if(!current.Keys.ToArray().Exists(x=>x.SequenceEqual(key))){
				current[key] = new TValue();
			}
			return current[key];
		}
		public static bool ContainsKey(this IDictionary current,string value,bool ignoreCase){
			value = value.ToLower();
			foreach(string key in current.Keys){
				if(key.ToLower() == value){
					return true;
				}
			}
			return false;
		}
		public static string GetKey(this Dictionary<KeyCode,string> current,string value){
			foreach(var item in current){
				string itemValue = Convert.ToString(item.Value);
				if(itemValue.Matches(value,true)){
					return Convert.ToString(item.Key);
				}
			}
			return "";
		}
		public static void RemoveValue<TKey,TValue>(this Dictionary<TKey,TValue> current,TValue value){
			foreach(var item in current.Copy()){
				if(item.Value.Equals(value)){
					current.Remove(item.Key);
				}
			}
		}
	}
}