using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Zios.Containers;
namespace Zios.Interface{
	public class Undo : Singleton{
		public static Undo instance;
		public static int position = -1;
		public static Hierarchy<Type,string,object> snapshot = new Hierarchy<Type,string,object>();
		public static Hierarchy<string,object> snapshotPrefs = new Hierarchy<string,object>();
		public List<string> buffer = new List<string>();
		private List<string> cache = new List<string>();
		private List<Action<string>> callback = new List<Action<string>>();
		public void OnEnable(){
			Undo.Setup();
			this.SetupHooks();
		}
		public static void Reset(){
			Undo.instance.buffer.Clear();
			Undo.instance.cache.Clear();
		}
		public static void Setup(){
			if(Undo.instance.IsNull()){
				Undo.instance = FileManager.GetAsset<Undo>("Undo.asset");
				if(!Undo.instance.IsNull()){
					Undo.Reset();
					UnityEditor.Undo.undoRedoPerformed += Undo.Process;
				}
			}
		}
		public static void Add(string operation,string undo,string redo,Action method){
			Undo.Add(operation,undo,redo,(x)=>method());
		}
		public static void Add(string operation,string undo,string redo,Action<string> method){
			Undo.Setup();
			if(Utility.IsBusy() || Undo.instance.IsNull() || (undo.IsEmpty() && redo.IsEmpty())){return;}
			var instance = Undo.instance;
			Utility.RecordObject(instance,operation);
			var seed = UnityEngine.Random.Range(0.0f,100.0f).ToString() + "!";
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
			var instance = Undo.instance;
			var total = instance.callback.Count()*2;
			if(instance.buffer.Count() > total){
				instance.buffer = instance.buffer.TakeRight(total).ToList();
			}
		}
		public static void Process(){
			var instance = Undo.instance;
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
		public static void RecordStart(Type target,bool clearPrefRecords=true){
			ObjectExtension.ResetCache();
			if(clearPrefRecords){Undo.snapshotPrefs.Clear();}
			Undo.snapshot[target] = target.GetVariables(null,ObjectExtension.staticPublicFlags);
		}
		public static void RecordPref<Type>(string name,Type value){
			var current = Utility.GetPref<Type>(name,value);
			if(!current.IsNull() && !current.Equals(value)){
				Undo.snapshotPrefs[name] = current;
			}
			Utility.SetPref<Type>(name,value);
		}
		public static void RecordEnd(string operation,Type target,Action callback,bool handlePrefs=true){
			Undo.RecordEnd(operation,target,(x)=>callback(),handlePrefs);
		}
		public static void RecordEnd(string operation,Type target,Action<string> callback=null,bool handlePrefs=true){
			ObjectExtension.ResetCache();
			var undo = "";
			var redo = "";
			if(Undo.snapshot.ContainsKey(target)){
				var changes = Undo.snapshot[target].Difference(target.GetVariables(null,ObjectExtension.staticPublicFlags));
				foreach(var item in changes){
					var scope = "&&&"+target.FullName+"###"+item.Key+"|||";
					undo += scope + Undo.snapshot[target][item.Key];
					redo += scope + item.Value;
				}
				Undo.snapshot.Remove(target);
			}
			if(handlePrefs){
				foreach(var pref in Undo.snapshotPrefs){
					var current = Utility.GetPref(pref.Key,pref.Value);
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
			ObjectExtension.ResetCache();
			foreach(var change in items){
				if(change.Trim().IsEmpty()){continue;}
				if(change.Contains("###")){
					var path = change.Split("###")[0];
					var field = change.Split("###")[1].Split("|||")[0];
					var value = change.Split("###")[1].Split("|||")[1];
					var scope = Utility.GetType(path);
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
					if(type.Contains("Bool")){Utility.SetPref<bool>(key,value.ToBool());}
					else if(type.Contains("Int")){Utility.SetPref<int>(key,value.ToInt());}
					else if(type.Contains("String")){Utility.SetPref<string>(key,value);}
					else if(type.Contains("Float")){Utility.SetPref<float>(key,value.ToFloat());}
				}
			}
			Undo.snapshotPrefs.Clear();
		}
	}
}