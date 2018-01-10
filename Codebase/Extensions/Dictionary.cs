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
			TValue output;
			if(!current.TryGetValue(key,out output)){
				return value;
			}
			return output;
		}
		public static void SetValues<TKey,TValue>(this IDictionary<TKey,TValue> current,IList<TValue> values) where TValue : new(){
			int index = 0;
			foreach(var key in current.Keys.ToList()){
				current[key] = values[index];
				++index;
			}
		}
		public static TValue AddDefault<TKey,TValue>(this IDictionary<TKey,TValue> current,TKey key){
			TValue output;
			if(!current.TryGetValue(key,out output)){
				current[key] = output = default(TValue);
			}
			return output;
		}
		public static TValue AddNew<TKey,TValue>(this IDictionary<TKey,TValue> current,TKey key) where TValue : new(){
			TValue output;
			if(!current.TryGetValue(key,out output)){
				current[key] = output = new TValue();
			}
			return output;
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
		public static Key GetKey<Key,Value>(this Dictionary<Key,Value> current,Value value){
			return current.FirstOrDefault(x=>x.Value.Equals(value)).Key;
		}
		public static void RemoveValue<TKey,TValue>(this Dictionary<TKey,TValue> current,TValue value){
			foreach(var item in current.Copy()){
				if(item.Value.Equals(value)){
					current.Remove(item.Key);
				}
			}
		}
		public static Dictionary<Key,Value> Merge<Key,Value>(this Dictionary<Key,Value> current,Dictionary<Key,Value> other){
			foreach(var item in other){
				current[item.Key] = item.Value;
			}
			return current;
		}
		public static Dictionary<Key,Value> Difference<Key,Value>(this Dictionary<Key,Value> current,Dictionary<Key,Value> other){
			var output = new Dictionary<Key,Value>();
			foreach(var item in other){
				var key = item.Key;
				Value value;
				if(current.TryGetValue(key,out value)){
					bool nullMatch = value.IsNull() && other[key].IsNull();
					bool referenceMatch = !nullMatch && !other[key].GetType().IsValueType;
					bool valueMatch = !nullMatch && other[key].Equals(current[key]);
					bool match = nullMatch || referenceMatch || valueMatch;
					/*if(current[key] is IEnumerable){
						match = current[key].As<IEnumerable>().SequenceEqual(other[key]);
					}*/
					if(match){continue;}
				}
				output[item.Key] = item.Value;
			}
			return output;
		}
	}
}