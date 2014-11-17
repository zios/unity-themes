using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public static class DictionaryExtension{
	public static Dictionary<TKey,TValue> Copy<TKey,TValue>(this Dictionary<TKey,TValue> current){
		return new Dictionary<TKey,TValue>(current);
	}
	public static void Setup<TKey,TValue>(this Dictionary<TKey,TValue> current,TKey value){
		if(!current.ContainsKey(value)){
			current[value] = default(TValue);
		}
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