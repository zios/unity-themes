using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Log{
	using Zios.Extensions;
	using Zios.Unity.Proxy;
	using UnityObject = UnityEngine.Object;
	public static class Log{
		private static Dictionary<object,int> messages = new Dictionary<object,int>();
		public static void Editor(string text){
			if(!Proxy.IsPlaying()){
				Log.Show(text);
			}
		}
		public enum LogType{Debug,Warning,Error};
		public static void Show(object key,string text,object target,LogType type,int limit){
			if(Log.messages.AddNew(key) < limit || limit == -1){
				Log.messages[key] += 1;
				var unityTarget = target as UnityObject;
				if(type==LogType.Debug){Debug.Log(text,unityTarget);}
				else if(type==LogType.Warning){Debug.LogWarning(text,unityTarget);}
				else if(type==LogType.Error){Debug.LogError(text,unityTarget);}
			}
		}
		public static void Show(object key,string text,object target=null,int limit=-1){Log.Show(key,text,target,LogType.Debug,limit);}
		public static void Show(string text,object target=null,int limit=-1){Log.Show(text,text,target,LogType.Debug,limit);}
		public static void Warning(object key,string text,object target=null,int limit=-1){Log.Show(key,text,target,LogType.Warning,limit);}
		public static void Warning(string text,object target=null,int limit=-1){Log.Show(text,text,target,LogType.Warning,limit);}
		public static void Error(object key,string text,object target=null,int limit=-1){Log.Show(key,text,target,LogType.Error,limit);}
		public static void Error(string text,object target=null,int limit=-1){Log.Show(text,text,target,LogType.Error,limit);}
	}
}