using System.Collections.Generic;
using UnityEditor;
namespace Zios.Unity.Editor.Pref{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	public static class EditorPref{
		public static Dictionary<string,object> cache = new Dictionary<string,object>();
		public static T Set<T>(string name,T value){
			EditorPref.cache[name] = value;
			if(value is bool){EditorPrefs.SetBool(name,value.As<bool>());}
			else if(value is int){EditorPrefs.SetInt(name,value.As<int>());}
			else if(value is float){EditorPrefs.SetFloat(name,value.As<float>());}
			else if(value is string){EditorPrefs.SetString(name,value.As<string>());}
			return value;
		}
		public static bool Has(string name){return EditorPrefs.HasKey(name);}
		public static T Get<T>(string name,T fallback=default(T)){
			if(EditorPref.cache.ContainsKey(name)){return EditorPref.cache[name].As<T>();}
			object value = fallback;
			if(fallback is bool){value = EditorPrefs.GetBool(name,fallback.As<bool>());}
			else if(fallback is int){value = EditorPrefs.GetInt(name,fallback.As<int>());}
			else if(fallback is float){value = EditorPrefs.GetFloat(name,fallback.As<float>());}
			else if(fallback is string){value = EditorPrefs.GetString(name,fallback.As<string>());}
			EditorPref.cache[name] = value;
			return value.As<T>();
		}
		public static T Build<T>(string name,T fallback=default(T)){
			return EditorPref.Has(name) ? EditorPref.Get<T>(name,fallback) : EditorPref.Set<T>(name,fallback);
		}
		public static void Clear(string name){
			EditorPref.cache.Remove(name);
			EditorPrefs.DeleteKey(name);
		}
		public static void AddCallback(string name,string methodPath){
			var callbacks = EditorPrefs.GetString(name);
			EditorPrefs.SetString(name,callbacks+"|"+methodPath);
		}
		public static void Call(string name,bool showWarnings=false){
			var callbacks = EditorPrefs.GetString(name);
			var called = new List<string>();
			var success = new List<string>();
			bool debug = Reflection.debug;
			Reflection.debug = showWarnings;
			foreach(var method in callbacks.Split("|")){
				if(called.Contains(method) || method.IsEmpty()){continue;}
				if(!method.CallPath().IsNull()){
					success.Add(method);
				}
				called.Add(method);
			}
			Reflection.debug = debug;
			var value = success.Count > 0 ? success.Join("|") : "";
			EditorPrefs.SetString(name,value);
		}
		public static void Toggle(string name,bool fallback=false){
			bool value = !EditorPref.Get(name,fallback);
			EditorPref.Set(name,value);
		}
		public static void ClearAll(bool prompt){
			if(!prompt || EditorUtility.DisplayDialog("Clear Editor Prefs","Delete all the editor preferences?","Yes","No")){
				EditorPrefs.DeleteAll();
			}
		}
		#if !ZIOS_MINIMAL
		[MenuItem("Zios/Prefs/Clear Editor")]
		public static void DeleteAll(){EditorPref.ClearAll(true);}
		#endif
	}
}