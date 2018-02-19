using System;
using System.Collections.Generic;
using System.Linq;
using UnityRandom = UnityEngine.Random;
using UnityUndo = UnityEditor.Undo;
namespace Zios.Unity.Editor.Undo{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Supports.Hierarchy;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Proxy;
	using Zios.Unity.Supports.Singleton;
	public class Undo : Singleton{
		public static Undo singleton;
		public static int position = -1;
		public static Hierarchy<Type,string,object> snapshot = new Hierarchy<Type,string,object>();
		public static Hierarchy<string,object> snapshotPrefs = new Hierarchy<string,object>();
		public List<string> buffer = new List<string>();
		private List<string> cache = new List<string>();
		private List<Action<string>> callback = new List<Action<string>>();
		public static Undo Get(){
			Undo.singleton = Undo.singleton ?? Singleton.Get<Undo>();
			return Undo.singleton;
		}
		public void OnEnable(){
			Undo.singleton = this;
			Undo.singleton.buffer.Clear();
			Undo.singleton.cache.Clear();
			UnityUndo.undoRedoPerformed += Undo.Process;
			this.SetupHooks();
		}
		public static void Add(string operation,string undo,string redo,Action method){
			Undo.Add(operation,undo,redo,(x)=>method());
		}
		public static void Add(string operation,string undo,string redo,Action<string> method){
			var instance = Undo.Get();
			if(Proxy.IsBusy() || instance.IsNull() || (undo.IsEmpty() && redo.IsEmpty())){return;}
			ProxyEditor.RecordObject(instance,operation);
			var seed = UnityRandom.Range(0.0f,100.0f).ToString() + "!";
			undo = seed + undo;
			redo = seed + redo;
			instance.buffer.Add(undo);
			instance.cache.Add(undo);
			instance.buffer.Add(redo);
			instance.cache.Add(redo);
			Undo.position = (instance.buffer.Count()/2)-1;
			instance.callback = instance.callback.Take(Undo.position).ToList();
			instance.callback.Add(method);
		}
		public static void CorrectBuffer(){
			var instance = Undo.Get();
			var total = instance.callback.Count()*2;
			if(instance.buffer.Count() > total){
				instance.buffer = instance.buffer.TakeRight(total).ToList();
			}
		}
		public static void Process(){
			var instance = Undo.Get();
			Undo.CorrectBuffer();
			if(Undo.position < instance.callback.Count()-1 && (instance.buffer.Count() > instance.cache.Count())){
				var change = instance.buffer.Except(instance.cache).LastOrDefault();
				if(!change.IsEmpty()){
					change = change.Split("!")[1];
					Undo.position += 1;
					instance.callback[Undo.position](change);
				}
			}
			else if(Undo.position > -1 && (instance.cache.Count() > instance.buffer.Count())){
				var change = instance.cache.Except(instance.buffer).FirstOrDefault();
				if(!change.IsEmpty()){
					change = change.Split("!")[1];
					instance.callback[Undo.position](change);
					Undo.position -= 1;
				}
			}
			instance.cache = instance.buffer.Copy();
		}
		public static void RecordStart<Type>(bool clearPrefRecords=true){Undo.RecordStart(typeof(Type),clearPrefRecords);}
		public static void RecordStart(Type target,bool clearPrefRecords=true){
			Reflection.ResetCache();
			if(clearPrefRecords){Undo.snapshotPrefs.Clear();}
			Undo.snapshot[target] = target.GetVariables(null,Reflection.staticPublicFlags);
		}
		public static void RecordPref<Type>(string name,Type value){
			var current = EditorPref.Get<Type>(name,value);
			if(!current.IsNull() && !current.Equals(value)){
				Undo.snapshotPrefs[name] = current;
			}
			EditorPref.Set<Type>(name,value);
		}
		public static void RecordEnd<Type>(string operation,Action callback,bool handlePrefs=true){
			Undo.RecordEnd(operation,typeof(Type),(x)=>callback(),handlePrefs);
		}
		public static void RecordEnd(string operation,Type target,Action callback,bool handlePrefs=true){
			Undo.RecordEnd(operation,target,(x)=>callback(),handlePrefs);
		}
		public static void RecordEnd(string operation,Type target,Action<string> callback=null,bool handlePrefs=true){
			Reflection.ResetCache();
			var undo = "";
			var redo = "";
			if(Undo.snapshot.ContainsKey(target)){
				var changes = Undo.snapshot[target].Difference(target.GetVariables(null,Reflection.staticPublicFlags));
				foreach(var item in changes){
					var scope = "&&&"+target.FullName+"###"+item.Key+"|||";
					undo += scope + Undo.snapshot[target][item.Key];
					redo += scope + item.Value;
				}
				Undo.snapshot.Remove(target);
			}
			if(handlePrefs){
				foreach(var pref in Undo.snapshotPrefs){
					var current = EditorPref.Get(pref.Key,pref.Value);
					var head = "&&&"+pref.Key+"|||"+pref.Value.GetType().Name+"---";
					undo += head+pref.Value.SerializeAuto();
					redo += head+current.SerializeAuto();
				}
				Undo.snapshotPrefs.Clear();
			}
			undo = undo.ReplaceFirst("&&&","").Trim();
			redo = redo.ReplaceFirst("&&&","").Trim();
			Undo.Add(operation,undo,redo,Undo.Handle+callback);
		}
		public static void Handle(string data){
			var items = data.Split("&&&");
			Reflection.ResetCache();
			foreach(var change in items){
				if(change.Trim().IsEmpty()){continue;}
				if(change.Contains("###")){
					var path = change.Split("###")[0];
					var field = change.Split("###")[1].Split("|||")[0];
					var value = change.Split("###")[1].Split("|||")[1];
					var scope = Reflection.GetType(path);
					var type = scope.GetVariableType(field);
					if(type == typeof(string)){scope.SetVariable(field,value);}
					else if(type == typeof(int)){scope.SetVariable(field,value.ToInt());}
					else if(type == typeof(float)){scope.SetVariable(field,value.ToFloat());}
					else if(type == typeof(bool)){scope.SetVariable(field,value.ToBool());}
					else if(type.IsEnum){scope.SetVariable(field,Enum.Parse(type,value));}
					Undo.snapshot.Remove(scope);
				}
				else{
					var key = change.Split("|||")[0];
					var type = change.Split("|||")[1].Split("---")[0];
					var value = change.Split("|||")[1].Split("---")[1];
					if(type.Contains("Bool")){EditorPref.Set<bool>(key,value.ToBool());}
					else if(type.Contains("Int")){EditorPref.Set<int>(key,value.ToInt());}
					else if(type.Contains("String")){EditorPref.Set<string>(key,value);}
					else if(type.Contains("Float")){EditorPref.Set<float>(key,value.ToFloat());}
				}
			}
			Undo.snapshotPrefs.Clear();
		}
	}
}