using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios.Unity.Call{
	using Zios.Unity.Proxy;
	using Zios.Unity.Time;
	using Zios.Extensions;
	#if UNITY_EDITOR
	using UnityEditor;
	using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
	#else
	public delegate void CallbackFunction();
	#endif
	public static class Call{
		private static Dictionary<object,KeyValuePair<Action,float>> delayedMethods = new Dictionary<object,KeyValuePair<Action,float>>();
		public static void Repeat(Action method,int amount){
			var repeat = Enumerable.Range(0,amount).GetEnumerator();
			while(repeat.MoveNext()){
				method();
			}
		}
		public static void Editor(Action method){
			#if UNITY_EDITOR
			if(!Proxy.IsPlaying()){
				method();
			}
			#endif
		}
		public static void Delay(Action method){
			#if UNITY_EDITOR
			if(Proxy.IsEditor()){
				CallbackFunction callback = new CallbackFunction(method);
				if(EditorApplication.delayCall != callback){
					EditorApplication.delayCall += callback;
				}
				return;
			}
			#endif
			Call.Delay(method,0);
		}
		public static void Delay(Action method,float seconds,bool overwrite=true){
			Call.Delay(method,method,seconds,overwrite);
		}
		public static void Delay(object key,Action method,float seconds,bool overwrite=true){
			if(!key.IsNull() && !method.IsNull()){
				if(seconds <= 0){
					method();
					return;
				}
				if(Call.delayedMethods.ContainsKey(key) && !overwrite){return;}
				Call.delayedMethods[key] = new KeyValuePair<Action,float>(method,Time.Get() + seconds);
			}
		}
		public static void CheckDelayed(){Call.CheckDelayed(false);}
		public static void CheckDelayed(bool editorCheck){
			if(editorCheck && Proxy.IsPlaying()){return;}
			if(!editorCheck && !Proxy.IsPlaying()){return;}
			if(Call.delayedMethods.Count < 1){return;}
			foreach(var item in Call.delayedMethods.Copy()){
				var method = item.Value.Key;
				float callTime = item.Value.Value;
				if(Time.Get() > callTime){
					method();
					Call.delayedMethods.Remove(item.Key);
				}
			}
		}
	}
}