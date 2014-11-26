#pragma warning disable 0162
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityObject = UnityEngine.Object;
using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
public class UtilityListener : AssetPostprocessor{
	public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved, string[] path){
		bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
		if(!playing && Utility.assetUpdate != null){
			Utility.assetUpdate();
		}
	}
}
#endif
public static class Utility{
	#if UNITY_EDITOR
	public static CallbackFunction assetUpdate;
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
		if(!Application.isPlaying){Object.DestroyImmediate(target);}
		else{Object.Destroy(target);}
	}
	public static void SetDirty(UnityObject target){
		#if UNITY_EDITOR
		EditorUtility.SetDirty(target);
		new SerializedObject(target).UpdateIfDirtyOrScript();
		#endif
	}
	public static GameObject FindPrefabRoot(GameObject target){
		#if UNITY_EDITOR
		return PrefabUtility.FindPrefabRoot(target);
		#endif
		return null;
	}
	public static void EditorUpdate(CallbackFunction method,bool callImmediately=false){
		#if UNITY_EDITOR
		bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
		if(!playing){
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
	public static void HierarchyUpdate(CallbackFunction method,bool callImmediately=false){
		#if UNITY_EDITOR
		bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
		if(!playing){
			if(!EditorApplication.hierarchyWindowChanged.Contains(method)){
				EditorApplication.hierarchyWindowChanged += method;
				if(callImmediately){method();}
			}
		}
		else{
			Utility.RemoveHierarchyUpdate(method);
		}
		#endif
	}
	public static void AssetUpdate(CallbackFunction method,bool callImmediately=false){
		#if UNITY_EDITOR
		bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
		if(!playing){
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
		Utility.assetUpdate -= method;
		#endif
	}
	public static void RemoveEditorUpdate(CallbackFunction method){
		#if UNITY_EDITOR
		EditorApplication.update -= method;
		#endif
	}
	public static void RemoveHierarchyUpdate(CallbackFunction method){
		#if UNITY_EDITOR
		EditorApplication.hierarchyWindowChanged -= method;
		#endif
	}
	public static void EditorCall(CallbackFunction method){
		#if UNITY_EDITOR
		bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
		if(!playing){
			method();
		}
		#endif
	}
}