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
	using Events;
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
			Event.Add("On Late Update",(Method)Utility.CheckLoaded);
			Event.Add("On Late Update",(Method)Utility.CheckDelayed);
			#if UNITY_EDITOR
			Event.Register("On Global Event");
			Event.Register("On Editor Update");
			Event.Register("On Prefab Changed");
			Event.Register("On Lightmap Baked");
			Event.Register("On Windows Reordered");
			Event.Register("On Hierarchy Changed");
			Event.Register("On Asset Changed");
			Event.Register("On Asset Saving");
			Event.Register("On Asset Creating");
			Event.Register("On Asset Deleting");
			Event.Register("On Asset Moving");
			Event.Register("On Scene Loaded");
			Event.Register("On Editor Scene Loaded");
			Event.Register("On Editor Quit");
			Event.Register("On Mode Changed");
			Event.Register("On Enter Play");
			Event.Register("On Exit Play");
			Event.Register("On Undo Flushing");
			Event.Register("On Undo");
			Event.Register("On Redo");
			#if UNITY_5
			Camera.onPostRender += (Camera camera)=>Event.Call("On Camera Post Render",camera);
			Camera.onPreRender += (Camera camera)=>Event.Call("On Camera Pre Render",camera);
			Camera.onPreCull += (Camera camera)=>Event.Call("On Camera Pre Cull",camera);
			Lightmapping.completed += ()=>Event.Call("On Lightmap Baked");
			#endif
			Undo.willFlushUndoRecord += ()=>Event.Call("On Undo Flushing");
			Undo.undoRedoPerformed += ()=>Event.Call("On Undo");
			Undo.undoRedoPerformed += ()=>Event.Call("On Redo");
			PrefabUtility.prefabInstanceUpdated += (GameObject target)=>Event.Call("On Prefab Changed",target);
			EditorApplication.projectWindowChanged += ()=>Event.Call("On Project Changed");
			EditorApplication.playmodeStateChanged += ()=>Event.Call("On Mode Changed");
			EditorApplication.playmodeStateChanged += ()=>{
				bool changing = EditorApplication.isPlayingOrWillChangePlaymode;
				bool playing = Application.isPlaying;
				if(changing && !playing){Event.Call("On Enter Play");}
				if(!changing && playing){Event.Call("On Exit Play");}
			};
			EditorApplication.hierarchyWindowChanged += ()=>Event.DelayCall("On Hierarchy Changed",0.25f);
			EditorApplication.update += ()=>Event.Call("On Editor Update");
			EditorApplication.update += ()=>Utility.CheckLoaded(true);
			EditorApplication.update += ()=>Utility.CheckDelayed(true);
			UnityAction editorQuitEvent = new UnityAction(()=>Event.Call("On Editor Quit"));
			CallbackFunction windowEvent = ()=>Event.Call("On Window Reordered");
			CallbackFunction globalEvent = ()=>Event.Call("On Global Event");
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
				Event.Call("On" + term + " Scene Loaded");
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
		public static Type GetType(string path){
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
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