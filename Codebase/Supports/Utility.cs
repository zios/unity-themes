#pragma warning disable 0162
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
public class UtilityListener : AssetPostprocessor{
	public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved, string[] path){
		bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
		if(!playing && Utility.assetUpdate != null){
			Utility.assetUpdate();
		}
	}
}
#else
	public delegate void CallbackFunction();
#endif
public static class Utility{
	#if UNITY_EDITOR
	public static CallbackFunction assetUpdate;
	public static List<CallbackFunction> hierarchyMethods = new List<CallbackFunction>();
	public static bool hierarchyPaused;
	#endif
	public static string AddRoot(this string current,Component parent){
		string prefix = parent.HasVariable("alias") ? parent.GetVariable<string>("alias") : parent.GetType().ToString();
		prefix = prefix.Split(".").Last();
		if(!current.StartsWith(prefix+"/")){
			current = prefix + "/" + current;
		}
		return current.Replace("//","/").TrimRight("/");
	}
	public static void Destroy(UnityObject target){
		if(!Application.isPlaying){UnityObject.DestroyImmediate(target,true);}
		else{UnityObject.Destroy(target);}
	}
	public static void SetDirty(UnityObject target){
		#if UNITY_EDITOR
		EditorUtility.SetDirty(target);
		//new SerializedObject(target).UpdateIfDirtyOrScript();
		#endif
	}
	public static GameObject FindPrefabRoot(GameObject target){
		#if UNITY_EDITOR
		return PrefabUtility.FindPrefabRoot(target);
		#endif
		return target;
	}
	public static void AddEditorUpdate(CallbackFunction method,bool callImmediately=false){
		#if UNITY_EDITOR
		if(!Utility.IsPlaying()){
			if(!EditorApplication.update.Contains(method)){
				EditorApplication.update += method;
				if(callImmediately){method();}
			}
		}
		else{
			Utility.RemoveEditorUpdate(method);
		}
		#endif
	}
	public static void PauseHierarchyUpdates(){
		#if UNITY_EDITOR
		foreach(CallbackFunction method in Utility.hierarchyMethods){
			EditorApplication.hierarchyWindowChanged -= method;
		}
		Utility.hierarchyPaused = true;
		#endif
	}
	public static void ResumeHierarchyUpdates(){
		#if UNITY_EDITOR
		foreach(CallbackFunction method in Utility.hierarchyMethods){
			if(!EditorApplication.hierarchyWindowChanged.Contains(method)){
				EditorApplication.hierarchyWindowChanged += method;
			}
		}
		Utility.hierarchyPaused = false;
		#endif
	}
	public static void AddHierarchyUpdate(CallbackFunction method,bool callImmediately=false){
		#if UNITY_EDITOR
		if(!Utility.IsPlaying()){
			if(!EditorApplication.hierarchyWindowChanged.Contains(method)){
				if(!Utility.hierarchyPaused){
					EditorApplication.hierarchyWindowChanged += method;
				}
				Utility.hierarchyMethods.Add(method);
				if(callImmediately){method();}
			}
		}
		else{
			Utility.RemoveHierarchyUpdate(method);
		}
		#endif
	}
	public static void AddAssetUpdate(CallbackFunction method,bool callImmediately=false){
		#if UNITY_EDITOR
		if(!Utility.IsPlaying()){
			if(!Utility.assetUpdate.Contains(method)){
				Utility.assetUpdate += method;
				if(callImmediately){method();}
			}
		}
		else{
			Utility.RemoveAssetUpdate(method);
		}
		#endif
	}
	public static void RemoveAssetUpdate(CallbackFunction method){
		#if UNITY_EDITOR
		while(Utility.assetUpdate.Contains(method)){
			Utility.assetUpdate -= method;
		}
		#endif
	}
	public static void RemoveEditorUpdate(CallbackFunction method){
		#if UNITY_EDITOR
		while(EditorApplication.update.Contains(method)){
			EditorApplication.update -= method;
		}
		#endif
	}
	public static void RemoveHierarchyUpdate(CallbackFunction method){
		#if UNITY_EDITOR
		while(EditorApplication.hierarchyWindowChanged.Contains(method)){
			EditorApplication.hierarchyWindowChanged -= method;
		}
		Utility.hierarchyMethods.RemoveAll(x=>x==method);
		#endif
	}
	public static void EditorCall(CallbackFunction method){
		#if UNITY_EDITOR
		if(!Utility.IsPlaying()){
			method();
		}
		#endif
	}
	public static void EditorDelayCall(CallbackFunction method){
		#if UNITY_EDITOR
		EditorApplication.delayCall += method;
		#endif
	}
	public static Type GetEditorType(string name){
		#if UNITY_EDITOR
		foreach(var type in typeof(EditorApplication).Assembly.GetTypes()){
			if(type.Name == name){return type;}
		}
		#endif
		return null;
	}
	public static bool IsPlaying(){
		#if UNITY_EDITOR
		return EditorApplication.isPlayingOrWillChangePlaymode;	
		#endif
		return Application.isPlaying;
	}
	public static int GetLocalID(int instanceID){
		#if UNITY_EDITOR
		return UnityEditor.Unsupported.GetLocalIdentifierInFile(instanceID);
		#endif
		return 0;
	}
	public static bool MoveComponentUp(Component component){
		#if UNITY_EDITOR
		return (bool)Utility.GetEditorType("ComponentUtility").CallMethod("MoveComponentUp",component.AsArray());
		#endif
		return false;
	}
	public static bool MoveComponentDown(Component component){
		#if UNITY_EDITOR
		return (bool)Utility.GetEditorType("ComponentUtility").CallMethod("MoveComponentDown",component.AsArray());
		#endif
		return false;
	}
	public static bool ReconnectToLastPrefab(GameObject target){
		#if UNITY_EDITOR
		return PrefabUtility.ReconnectToLastPrefab(target);
		#endif
		return false;
	}
	public static void DisconnectPrefabInstance(UnityObject target){
		#if UNITY_EDITOR
		PrefabUtility.DisconnectPrefabInstance(target);
		#endif
	}
	public static void EditorLog(string text){
		if(!Application.isPlaying){
			Debug.Log(text);
		}
	}
}