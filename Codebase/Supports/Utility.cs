#pragma warning disable 0162
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
namespace Zios{
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEditorInternal;
    using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
    public class UtilityListener : AssetPostprocessor{
	    public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved, string[] path){
		    bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
		    if(!playing){Events.Call("On Asset Changed");}
	    }
    }
    [InitializeOnLoad]
    #else
	    public delegate void CallbackFunction();
    #endif
    public static class Utility{
		//=================
		// Editor Only
		//=================
	    #if UNITY_EDITOR
		public static float sceneCheck;
	    public static EditorWindow[] inspectors;
	    public static List<CallbackFunction> hierarchyMethods = new List<CallbackFunction>();
	    public static Dictionary<CallbackFunction,float> delayedMethods = new Dictionary<CallbackFunction,float>();
		public static List<UnityObject> delayedDirty = new List<UnityObject>();
		public static Dictionary<UnityObject,SerializedObject> serializedObjects = new Dictionary<UnityObject,SerializedObject>();
		public static int ignoredHierarchyChange = 0;
		public static bool ignoreFirstHierarchyChange = true;
	    public static bool hierarchyPaused;
		public static bool delayPaused;
		public static bool delayProcessing;
	    static Utility(){
			Events.Register("On Global Event");
			Events.Register("On Windows Reordered");
			Events.Register("On Asset Changed");
			Events.Register("On Scene Loaded");
			Events.Register("On Enter Play");
			Events.Register("On Exit Play");
			EditorApplication.update += ()=>Events.Call("On Editor Update");
			EditorApplication.hierarchyWindowChanged += ()=>{
				if(Utility.ignoreFirstHierarchyChange && Utility.ignoredHierarchyChange < 1){
					Utility.ignoredHierarchyChange = 1;
					return;
				}
				Events.Call("On Hierarchy Changed");
			};
			EditorApplication.projectWindowChanged += ()=>Events.Call("On Project Changed");
			EditorApplication.playmodeStateChanged += ()=>Events.Call("On Mode Changed");
			CallbackFunction windowEvent = ()=>Events.Call("On Window Reordered");
			CallbackFunction globalEvent = ()=>Events.Call("On Global Event");
			var windowsReordered = typeof(EditorApplication).GetVariable<CallbackFunction>("windowsReordered");
			typeof(EditorApplication).SetVariable("windowsReordered",windowsReordered+windowEvent);
			var globalEventHandler = typeof(EditorApplication).GetVariable<CallbackFunction>("globalEventHandler");
			typeof(EditorApplication).SetVariable("globalEventHandler",globalEventHandler+globalEvent);
			EditorApplication.playmodeStateChanged += ()=>{
				bool changing = EditorApplication.isPlayingOrWillChangePlaymode;
				bool playing = Application.isPlaying;
				Utility.ignoredHierarchyChange = 0;
				if(changing && !playing){Events.Call("On Enter Play");}
				if(!changing && playing){Events.Call("On Exit Play");}
			};
			EditorApplication.update += ()=>{
				if(Time.realtimeSinceStartup < 0.5 && Utility.sceneCheck == 0){
					Events.Call("On Scene Loaded");
					Utility.sceneCheck = 1;
				}
				if(Time.realtimeSinceStartup > Utility.sceneCheck){
					Utility.sceneCheck = 0;
				}
			};
			EditorApplication.update += ()=>{
				if(Utility.delayedMethods.Count < 1){return;}
				Utility.delayProcessing = true;
				var complete = new List<CallbackFunction>();
				foreach(var item in Utility.delayedMethods){
					var method = item.Key;
					float callTime = item.Value;
					if(Time.realtimeSinceStartup > callTime){
						method();
						complete.Add(method);
					}
				}
				foreach(var method in complete){
					Utility.delayedMethods.Remove(method);
				}
				Utility.delayProcessing = false;
			};
		}
		//=================
		// Editor-Only
		//=================
	    public static SerializedObject GetSerializedObject(UnityObject target){
			if(!Utility.serializedObjects.ContainsKey(target)){
				Utility.serializedObjects[target] = new SerializedObject(target);
			}
			return Utility.serializedObjects[target];
		}
	    public static SerializedObject GetSerialized(UnityObject target){
			Type type = typeof(SerializedObject);
		    return type.CallMethod<SerializedObject>("LoadFromCache",target.GetInstanceID());
	    }
		public static void UpdateSerialized(UnityObject target){
			var serialized = Utility.GetSerializedObject(target);
			serialized.Update();
			serialized.ApplyModifiedProperties();
			Utility.UpdatePrefab(target);
		}
	    public static EditorWindow[] GetInspectors(){
		    if(Utility.inspectors == null){
			    Type inspectorType = Utility.GetEditorType("InspectorWindow");
			    Utility.inspectors = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
		    }
			return Utility.inspectors;
	    }
	    public static Vector2 GetInspectorScrollPosition(this Rect current){
			Type inspectorWindow = Utility.GetEditorType("InspectorWindow");
			var window = EditorWindow.GetWindowWithRect(inspectorWindow,current);
			return window.GetVariable<Vector2>("m_ScrollPosition");
	    }
		[MenuItem("Zios/Process/Clear Player Prefs")]
		public static void DeletePlayerPrefs(){
			if(EditorUtility.DisplayDialog("Clear Player Prefs","Delete all the player preferences?","Yes","No")){
				PlayerPrefs.DeleteAll();
			}
		}
		[MenuItem("Zios/Process/Clear Editor Prefs")]
		public static void DeleteEditorPrefs(){
			if(EditorUtility.DisplayDialog("Clear Editor Prefs","Delete all the editor preferences?","Yes","No")){
				EditorPrefs.DeleteAll();
			}
		}
		#endif
		//=================
		// General
		//=================
		public static void TogglePlayerPref(string name,bool fallback=false){
			bool value = !(PlayerPrefs.GetInt(name) == fallback.ToInt());
			PlayerPrefs.SetInt(name,value.ToInt());
		}
		public static void ToggleEditorPref(string name,bool fallback=false){
		    #if UNITY_EDITOR
			bool value = !EditorPrefs.GetBool(name,fallback);
			EditorPrefs.SetBool(name,value);
			#endif
		}
	    public static void Destroy(UnityObject target){
		    if(!Application.isPlaying){UnityObject.DestroyImmediate(target,true);}
		    else{UnityObject.Destroy(target);}
	    }
	    public static Type GetEditorType(string name){
		    #if UNITY_EDITOR
		    foreach(var type in typeof(EditorApplication).Assembly.GetTypes()){
			    if(type.Name == name){return type;}
		    }
		    #endif
		    return null;
	    }
	    public static void EditorLog(string text){
		    if(!Application.isPlaying){
			    Debug.Log(text);
		    }
	    }
		//=================
		// Editor Call
		//=================
	    public static void EditorCall(CallbackFunction method){
		    #if UNITY_EDITOR
		    if(!Utility.IsPlaying()){
			    method();
		    }
		    #endif
	    }
	    public static void EditorDelayCall(CallbackFunction method){
			#if UNITY_EDITOR
			if(!Utility.IsPlaying() && !Utility.delayPaused){
				EditorApplication.delayCall += method;
				
			}
			#endif
	    }
	    public static void EditorDelayCall(CallbackFunction method,float seconds){
			#if UNITY_EDITOR
			if(!Utility.delayProcessing && !Utility.delayPaused){
				Utility.delayedMethods[method] = Time.realtimeSinceStartup + seconds;
			}
			#endif
	    }
		//=================
		// Proxy
		//=================
	    public static UnityObject GetPrefab(UnityObject target){
		    #if UNITY_EDITOR
		    return PrefabUtility.GetPrefabObject(target);
		    #endif
		    return null;
	    }
	    public static GameObject GetPrefabRoot(GameObject target){
		    #if UNITY_EDITOR
		    return PrefabUtility.FindPrefabRoot(target);
		    #endif
		    return null;
	    }
	    public static bool IsPlaying(){
		    #if UNITY_EDITOR
		    return Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;	
		    #endif
		    return Application.isPlaying;
	    }
	    public static bool IsPaused(){
		    #if UNITY_EDITOR
		    return EditorApplication.isPaused;	
		    #endif
		    return false;
	    }
	    public static void RepaintInspectors(){
		    #if UNITY_EDITOR
			Type inspectorType = Utility.GetEditorType("InspectorWindow");
			inspectorType.CallMethod("RepaintAllInspectors");
			#endif
	    }
		public static void ClearDirty(){Utility.delayedDirty.Clear();}
	    public static void SetDirty(UnityObject target,bool delayed=false,bool forced=false){
		    #if UNITY_EDITOR
			if(!forced && target.IsNull()){return;}
			if(!forced && target.GetPrefab().IsNull()){return;}
			if(delayed){
				if(!Utility.delayedDirty.Contains(target)){
					Events.AddLimited("On Enter Play",()=>Utility.SetDirty(target),1);
					Events.AddLimited("On Enter Play",Utility.ClearDirty,1);
					Utility.delayedDirty.AddNew(target);
				}
				return;
			}
		    EditorUtility.SetDirty(target);
			Utility.UpdatePrefab(target);
		    #endif
	    }
	    public static bool IsDirty(UnityObject target){
		    #if UNITY_EDITOR
		    return typeof(EditorUtility).CallMethod<bool>("IsDirty",target.GetInstanceID());
			#endif
			return false;
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
		public static void UpdatePrefab(UnityObject target){
		    #if UNITY_EDITOR
		    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
		    #endif
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
    }
}