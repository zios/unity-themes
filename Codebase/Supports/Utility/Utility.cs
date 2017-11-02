#pragma warning disable 0162
#pragma warning disable 0618
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityAction = UnityEngine.Events.UnityAction;
namespace Zios{
	using Event;
	#if UNITY_EDITOR
	using UnityEditor;
	using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
	#else
	public delegate void CallbackFunction();
	#endif
	[InitializeOnLoad]
	public static partial class Utility{
		private static float sceneCheck;
		private static Dictionary<object,int> messages = new Dictionary<object,int>();
		private static Dictionary<object,KeyValuePair<Action,float>> delayedMethods = new Dictionary<object,KeyValuePair<Action,float>>();
		private static Dictionary<string,Type> internalTypes = new Dictionary<string,Type>();
		static Utility(){Utility.Setup();}
		public static void Setup(){
			Events.Add("On Late Update",(Method)Utility.CheckLoaded);
			Events.Add("On Late Update",(Method)Utility.CheckDelayed);
			#if UNITY_EDITOR
			Events.Register("On Global Event");
			Events.Register("On Editor Update");
			Events.Register("On Prefab Changed");
			Events.Register("On Lightmap Baked");
			Events.Register("On Windows Reordered");
			Events.Register("On Hierarchy Changed");
			Events.Register("On Asset Changed");
			Events.Register("On Asset Saving");
			Events.Register("On Asset Creating");
			Events.Register("On Asset Deleting");
			Events.Register("On Asset Moving");
			Events.Register("On Scene Loaded");
			Events.Register("On Editor Scene Loaded");
			Events.Register("On Editor Quit");
			Events.Register("On Mode Changed");
			Events.Register("On Enter Play");
			Events.Register("On Exit Play");
			Events.Register("On Undo Flushing");
			Events.Register("On Undo");
			Events.Register("On Redo");
			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			Camera.onPostRender += (Camera camera)=>Events.Call("On Camera Post Render",camera);
			Camera.onPreRender += (Camera camera)=>Events.Call("On Camera Pre Render",camera);
			Camera.onPreCull += (Camera camera)=>Events.Call("On Camera Pre Cull",camera);
			Lightmapping.completed += ()=>Events.Call("On Lightmap Baked");
			#endif
			Undo.willFlushUndoRecord += ()=>Events.Call("On Undo Flushing");
			Undo.undoRedoPerformed += ()=>Events.Call("On Undo");
			Undo.undoRedoPerformed += ()=>Events.Call("On Redo");
			PrefabUtility.prefabInstanceUpdated += (GameObject target)=>Events.Call("On Prefab Changed",target);
			EditorApplication.projectWindowChanged += ()=>Events.Call("On Project Changed");
			EditorApplication.playModeStateChanged += (PlayModeStateChange state)=>{
				Events.Call("On Mode Changed");
				bool changing = EditorApplication.isPlayingOrWillChangePlaymode;
				bool playing = Application.isPlaying;
				if(changing && !playing){Events.Call("On Enter Play");}
				if(!changing && playing){Events.Call("On Exit Play");}
			};
			EditorApplication.hierarchyWindowChanged += ()=>Events.DelayCall("On Hierarchy Changed",0.25f);
			EditorApplication.update += ()=>Events.Call("On Editor Update");
			EditorApplication.update += ()=>Utility.CheckLoaded(true);
			EditorApplication.update += ()=>Utility.CheckDelayed(true);
			UnityAction editorQuitEvent = new UnityAction(()=>Events.Call("On Editor Quit"));
			CallbackFunction windowEvent = ()=>Events.Call("On Window Reordered");
			CallbackFunction globalEvent = ()=>Events.Call("On Global Event");
			var windowsReordered = typeof(EditorApplication).GetVariable<CallbackFunction>("windowsReordered");
			typeof(EditorApplication).SetVariable("windowsReordered",windowsReordered+windowEvent);
			var globalEventHandler = typeof(EditorApplication).GetVariable<CallbackFunction>("globalEventHandler");
			typeof(EditorApplication).SetVariable("globalEventHandler",globalEventHandler+globalEvent);
			var editorQuitHandler = typeof(EditorApplication).GetVariable<UnityAction>("editorApplicationQuit");
			typeof(EditorApplication).SetVariable("editorApplicationQuit",editorQuitHandler+editorQuitEvent);
			#endif
		}
		public static void CheckLoaded(){Utility.CheckLoaded(false);}
		public static void CheckLoaded(bool editor){
			if(editor && Application.isPlaying){return;}
			if(!editor && !Application.isPlaying){return;}
			if(Time.realtimeSinceStartup < 0.5 && Utility.sceneCheck == 0){
				var term = editor ? " Editor" : "";
				Events.Call("On" + term + " Scene Loaded");
				Utility.sceneCheck = 1;
			}
			if(Time.realtimeSinceStartup > Utility.sceneCheck){
				Utility.sceneCheck = 0;
			}
		}
		public static void CheckDelayed(){Utility.CheckDelayed(false);}
		public static void CheckDelayed(bool editorCheck){
			if(editorCheck && Application.isPlaying){return;}
			if(!editorCheck && !Application.isPlaying){return;}
			if(Utility.delayedMethods.Count < 1){return;}
			foreach(var item in Utility.delayedMethods.Copy()){
				var method = item.Value.Key;
				float callTime = item.Value.Value;
				if(Time.realtimeSinceStartup > callTime){
					method();
					Utility.delayedMethods.Remove(item.Key);
				}
			}
		}
		public static bool IsRepainting(){
			return UnityEngine.Event.current.type == EventType.Repaint;
		}
		//============================
		// General
		//============================
		public static void Destroy(UnityObject target,bool destroyAssets=false){
			if(target.IsNull()){return;}
			if(target is Component){
				var component = target.As<Component>();
				if(component.gameObject.IsNull()){return;}
			}
			if(!Application.isPlaying){UnityObject.DestroyImmediate(target,destroyAssets);}
			else{UnityObject.Destroy(target);}
		}
		public static List<Type> GetTypes<T>(){
			var assemblies = ObjectExtension.GetAssemblies();
			var matches = new List<Type>();
			foreach(var assembly in assemblies){
				var types = assembly.GetTypes();
				foreach(var type in types){
					if(type.IsSubclassOf(typeof(T))){
						matches.Add(type);
					}
				}
			}
			return matches;
		}
		public static Type GetType(string path){
			var assemblies = ObjectExtension.GetAssemblies();
			foreach(var assembly in assemblies){
				Type[] types = assembly.GetTypes();
				foreach(Type type in types){
					if(type.FullName == path){
						return type;
					}
				}
			}
			return null;
		}
		public static void ResetTypeCache(){Utility.internalTypes.Clear();}
		public static Type GetUnityType(string name){
			if(Utility.internalTypes.ContainsKey(name)){return Utility.internalTypes[name];}
			#if UNITY_EDITOR
			var fullCheck = name.ContainsAny(".","+");
			var alternative = name.ReplaceLast(".","+");
			var term = alternative.Split("+").Last();
			foreach(var type in typeof(UnityEditor.Editor).Assembly.GetTypes()){
				bool match = fullCheck && (type.FullName.Contains(name) || type.FullName.Contains(alternative)) && term.Matches(type.Name,true);
				if(type.Name == name || match){
					Utility.internalTypes[name] = type;
					return type;
				}
			}
			foreach(var type in typeof(UnityEngine.Object).Assembly.GetTypes()){
				bool match = fullCheck && (type.FullName.Contains(name) || type.FullName.Contains(alternative)) && term.Matches(type.Name,true);
				if(type.Name == name || match){
					Utility.internalTypes[name] = type;
					return type;
				}
			}
			#endif
			return null;
		}
		//============================
		// Logging
		//============================
		public static void EditorLog(string text){
			if(!Application.isPlaying){
				Debug.Log(text);
			}
		}
		public enum LogType{Debug,Warning,Error};
		public static void Log(object key,string text,UnityObject target,LogType type,int limit){
			if(Utility.messages.AddNew(key) < limit || limit == -1){
				Utility.messages[key] += 1;
				if(type==LogType.Debug){Debug.Log(text,target);}
				else if(type==LogType.Warning){Debug.LogWarning(text,target);}
				else if(type==LogType.Error){Debug.LogError(text,target);}
			}
		}
		public static void LogWarning(object key,string text,UnityObject target=null,int limit=-1){Utility.Log(key,text,target,LogType.Warning,limit);}
		public static void LogError(object key,string text,UnityObject target=null,int limit=-1){Utility.Log(key,text,target,LogType.Error,limit);}
		public static void LogWarning(string text,UnityObject target=null,int limit=-1){Utility.Log(text,text,target,LogType.Warning,limit);}
		public static void LogError(string text,UnityObject target=null,int limit=-1){Utility.Log(text,text,target,LogType.Error,limit);}
		//============================
		// Callbacks
		//============================
		public static void RepeatCall(CallbackFunction method,int amount){
			var repeat = Enumerable.Range(0,amount).GetEnumerator();
			while(repeat.MoveNext()){
				method();
			}
		}
		public static void EditorCall(Action method){
			#if UNITY_EDITOR
			if(!Utility.IsPlaying()){
				method();
			}
			#endif
		}
		public static void DelayCall(Action method){
			#if UNITY_EDITOR
			CallbackFunction callback = new CallbackFunction(method);
			if(EditorApplication.delayCall != callback){
				EditorApplication.delayCall += callback;
			}
			return;
			#endif
			Utility.DelayCall(method,0);
		}
		public static void DelayCall(Action method,float seconds,bool overwrite=true){
			Utility.DelayCall(method,method,seconds,overwrite);
		}
		public static void DelayCall(object key,Action method,float seconds,bool overwrite=true){
			if(!key.IsNull() && !method.IsNull()){
				if(seconds <= 0){
					method();
					return;
				}
				if(Utility.delayedMethods.ContainsKey(key) && !overwrite){return;}
				Utility.delayedMethods[key] = new KeyValuePair<Action,float>(method,Time.realtimeSinceStartup + seconds);
			}
		}
	}
}