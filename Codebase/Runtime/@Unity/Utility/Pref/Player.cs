using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Pref{
	using Zios.Extensions.Convert;
	using Zios.Unity.Extensions.Convert;
	public static class PlayerPref{
		public static Dictionary<string,object> cachePlayer = new Dictionary<string,object>();
		public static bool Has(string name){return PlayerPrefs.HasKey(name);}
		public static void Set<T>(string name,T value){
			PlayerPref.cachePlayer[name] = value;
			if(value is bool){PlayerPrefs.SetInt(name,value.As<bool>().ToInt());}
			else if(value is int){PlayerPrefs.SetInt(name,value.As<int>());}
			else if(value is float){PlayerPrefs.SetFloat(name,value.As<float>());}
			else if(value is string){PlayerPrefs.SetString(name,value.As<string>());}
			else if(value is Vector3){PlayerPrefs.SetString(name,value.As<Vector3>().ToString());}
			else if(value is byte){PlayerPrefs.SetString(name,value.As<byte>().ToString());}
			else if(value is short){PlayerPrefs.SetInt(name,value.As<short>().ToInt());}
			else if(value is double){PlayerPrefs.SetFloat(name,value.As<double>().ToFloat());}
			else if(value is ICollection){PlayerPrefs.SetString(name,value.As<IEnumerable>().SerializeAuto());}
			else if(typeof(T).IsEnum){PlayerPrefs.SetInt(name,value.As<int>());}
		}
		public static T Get<T>(string name,T fallback=default(T)){
			if(PlayerPref.cachePlayer.ContainsKey(name)){return PlayerPref.cachePlayer[name].As<T>();}
			object value = fallback;
			if(fallback is bool){value = PlayerPrefs.GetInt(name,fallback.As<bool>().ToInt()).ToBool();}
			else if(fallback is int){value = PlayerPrefs.GetInt(name,fallback.As<int>());}
			else if(fallback is float){value = PlayerPrefs.GetFloat(name,fallback.As<float>());}
			else if(fallback is string){value = PlayerPrefs.GetString(name,fallback.As<string>());}
			else if(fallback is Vector3){value = PlayerPrefs.GetString(name,fallback.As<Vector3>().Serialize());}
			else if(fallback is byte){value = PlayerPrefs.GetString(name,fallback.As<byte>().Serialize());}
			else if(fallback is short){value = PlayerPrefs.GetInt(name,fallback.As<short>().ToInt());}
			else if(fallback is double){value = PlayerPrefs.GetFloat(name,fallback.As<double>().ToFloat());}
			else if(fallback is ICollection){value = PlayerPrefs.GetString(name,fallback.As<IEnumerable>().SerializeAuto());}
			else if(typeof(T).IsEnum){value = PlayerPrefs.GetInt(name,fallback.As<int>());}
			PlayerPref.cachePlayer[name] = value;
			return value.As<T>();
		}
		public static void Toggle(string name,bool fallback=false){
			bool value = !(PlayerPref.Get<int>(name) == fallback.ToInt());
			PlayerPref.Set<int>(name,value.ToInt());
		}
		public static void ClearAll(){
			PlayerPrefs.DeleteAll();
		}
	}
}