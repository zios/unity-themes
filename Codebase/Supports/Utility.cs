#pragma warning disable 0162
#pragma warning disable 0618
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	using Events;
	#if UNITY_EDITOR
	using UnityEditor;
	using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
	public class UtilityListener : AssetPostprocessor{
		public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] movedTo, string[] movedFrom){
			bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
			if(!playing){Event.Call("On Asset Changed");}
		}
	}
	public class UtilityModificationListener : AssetModificationProcessor{
		public static string[] OnWillSaveAssets(string[] paths){
			foreach(string path in paths){Debug.Log("Saving Changes : " + path);}
			if(paths.Exists(x=>x.Contains(".unity"))){Event.Call("On Scene Saving");}
			Event.Call("On Asset Saving");
			return paths;
		}
		public static string OnWillCreateAssets(string path){
			Debug.Log("Creating : " + path);
			Event.Call("On Asset Creating");
			return path;
		}
		public static string[] OnWillDeleteAssets(string[] paths,RemoveAssetOptions option){
			foreach(string path in paths){Debug.Log("Deleting : " + path);}
			Event.Call("On Asset Deleting");
			return paths;
		}
		public static string OnWillMoveAssets(string path,string destination){
			Debug.Log("Moving : " + path + " to " + destination);
			Event.Call("On Asset Moving");
			return path;
		}
	}
	#else
		public delegate void CallbackFunction();
	#endif
	[InitializeOnLoad]
	public static class Utility{
		//============================
		// Editor Only
		//============================
		private static float sceneCheck;
		private static Dictionary<object,KeyValuePair<CallbackFunction,float>> delayedMethods = new Dictionary<object,KeyValuePair<CallbackFunction,float>>();
		#if UNITY_EDITOR
		private static EditorWindow[] inspectors;
		private static List<UnityObject> delayedDirty = new List<UnityObject>();
		private static Dictionary<UnityObject,SerializedObject> serializedObjects = new Dictionary<UnityObject,SerializedObject>();
		#endif
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
			Event.Register("On Mode Changed");
			Event.Register("On Enter Play");
			Event.Register("On Exit Play");
			Event.Register("On Undo Flushing");
			Event.Register("On Undo");
			Event.Register("On Redo");
			Camera.onPostRender += (Camera camera)=>Event.Call("On Camera Post Render",camera);
			Camera.onPreRender += (Camera camera)=>Event.Call("On Camera Pre Render",camera);
			Camera.onPreCull += (Camera camera)=>Event.Call("On Camera Pre Cull",camera);
			Undo.willFlushUndoRecord += ()=>Event.Call("On Undo Flushing");
			Undo.undoRedoPerformed += ()=>Event.Call("On Undo");
			Undo.undoRedoPerformed += ()=>Event.Call("On Redo");
			PrefabUtility.prefabInstanceUpdated += (GameObject target)=>Event.Call("On Prefab Changed",target);
			Lightmapping.completed += ()=>Event.Call("On Lightmap Baked");
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
			CallbackFunction windowEvent = ()=>Event.Call("On Window Reordered");
			CallbackFunction globalEvent = ()=>Event.Call("On Global Event");
			var windowsReordered = typeof(EditorApplication).GetVariable<CallbackFunction>("windowsReordered");
			typeof(EditorApplication).SetVariable("windowsReordered",windowsReordered+windowEvent);
			var globalEventHandler = typeof(EditorApplication).GetVariable<CallbackFunction>("globalEventHandler");
			typeof(EditorApplication).SetVariable("globalEventHandler",globalEventHandler+globalEvent);
			#endif
		}
		public static void CheckLoaded(){Utility.CheckLoaded(false);}
		public static void CheckLoaded(bool editor){
			if(editor && Application.isPlaying){return;}
			if(!editor && !Application.isPlaying){return;}
			if(Time.realtimeSinceStartup < 0.5 && Utility.sceneCheck == 0){
				Event.Call("On Scene Loaded");
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
		#if UNITY_EDITOR
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
			//Utility.UpdatePrefab(target);
		}
		public static EditorWindow[] GetInspectors(){
			if(Utility.inspectors == null){
				Type inspectorType = Utility.GetInternalType("InspectorWindow");
				Utility.inspectors = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			}
			return Utility.inspectors;
		}
		public static Vector2 GetInspectorScroll(){
			Type inspectorWindow = Utility.GetInternalType("InspectorWindow");
			var window = EditorWindow.GetWindow(inspectorWindow);
			return window.GetVariable<Vector2>("m_ScrollPosition");
		}
		public static Vector2 GetInspectorScroll(this Rect current){
			Type inspectorWindow = Utility.GetInternalType("InspectorWindow");
			var window = EditorWindow.GetWindowWithRect(inspectorWindow,current);
			return window.GetVariable<Vector2>("m_ScrollPosition");
		}
		[MenuItem("Zios/Process/Prefs/Clear Player")]
		public static void DeletePlayerPrefs(){
			if(EditorUtility.DisplayDialog("Clear Player Prefs","Delete all the player preferences?","Yes","No")){
				PlayerPrefs.DeleteAll();
			}
		}
		[MenuItem("Zios/Process/Prefs/Clear Editor")]
		public static void DeleteEditorPrefs(){
			if(EditorUtility.DisplayDialog("Clear Editor Prefs","Delete all the editor preferences?","Yes","No")){
				EditorPrefs.DeleteAll();
			}
		}
		[MenuItem("Zios/Process/Format Code")]
		public static void FormatCode(){
			var output = new StringBuilder();
			var current = "";
			foreach(var file in FileManager.FindAll("*.cs")){
				var contents = file.GetText();
				output.Clear();
				foreach(var line in contents.GetLines()){
					var leading = line.Substring(0,line.TakeWhile(char.IsWhiteSpace).Count()).Replace("    ","\t");
					current = leading+line.Trim();
					if(line.Trim().IsEmpty()){continue;}
					output.AppendLine(current);
				}
				file.WriteText(output.ToString().TrimEnd(null));
			}
		}
		#endif
		//============================
		// General
		//============================
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
			if(target.IsNull()){return;}
			if(target is Component){
				var component = target.As<Component>();
				if(component.gameObject.IsNull()){return;}
			}
			if(!Application.isPlaying){UnityObject.DestroyImmediate(target,true);}
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
		public static Type GetInternalType(string name){
			#if UNITY_EDITOR
			foreach(var type in typeof(UnityEditor.Editor).Assembly.GetTypes()){
				if(type.Name == name){return type;}
			}
			foreach(var type in typeof(UnityEngine.Object).Assembly.GetTypes()){
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
		//============================
		// Editor Call
		//============================
		public static void EditorCall(CallbackFunction method){
			#if UNITY_EDITOR
			if(!Utility.IsPlaying()){
				method();
			}
			#endif
		}
		public static void DelayCall(CallbackFunction method){
			#if UNITY_EDITOR
			if(!Utility.IsPlaying() && EditorApplication.delayCall != method){
				EditorApplication.delayCall += method;
			}
			return;
			#endif
			Utility.DelayCall(method,0);
		}
		public static void DelayCall(CallbackFunction method,float seconds){
			Utility.DelayCall(method,method,seconds);
		}
		public static void DelayCall(object key,CallbackFunction method,float seconds){
			if(!key.IsNull() && !method.IsNull()){
				Utility.delayedMethods[key] = new KeyValuePair<CallbackFunction,float>(method,Time.realtimeSinceStartup + seconds);
			}
		}
		//============================
		// Proxy - EditorUtility
		//============================
		public static bool DisplayCancelableProgressBar(string title,string message,float percent){
			#if UNITY_EDITOR
			return EditorUtility.DisplayCancelableProgressBar(title,message,percent);
			#endif
			return true;
		}
		public static void ClearProgressBar(){
			#if UNITY_EDITOR
			EditorUtility.ClearProgressBar();
			#endif
		}
		//============================
		// Proxy - AssetDatabase
		//============================
		public static void StartAssetEditing(){
			#if UNITY_EDITOR
			AssetDatabase.StartAssetEditing();
			#endif
		}
		public static void StopAssetEditing(){
			#if UNITY_EDITOR
			AssetDatabase.StopAssetEditing();
			#endif
		}
		public static void RefreshAssets(){
			#if UNITY_EDITOR
			AssetDatabase.Refresh();
			#endif
		}
		public static void SaveAssets(){
			#if UNITY_EDITOR
			AssetDatabase.SaveAssets();
			#endif
		}
		public static void ImportAsset(string path){
			#if UNITY_EDITOR
			AssetDatabase.ImportAsset(path);
			#endif
		}
		public static void DeleteAsset(string path){
			#if UNITY_EDITOR
			AssetDatabase.DeleteAsset(path);
			#endif
		}
		//============================
		// Proxy - PrefabUtility
		//============================
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
		public static void ApplyPrefab(GameObject target){
			#if UNITY_EDITOR
			GameObject root = PrefabUtility.FindPrefabRoot(target);
			PrefabUtility.ReplacePrefab(root,PrefabUtility.GetPrefabParent(root),ReplacePrefabOptions.ConnectToPrefab);
			#endif
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
		//============================
		// Proxy - EditorApplication
		//============================
		public static bool IsPaused(){
			#if UNITY_EDITOR
			return EditorApplication.isPaused;
			#endif
			return false;
		}
		public static bool IsBusy(){
			#if UNITY_EDITOR
			return EventDetector.loading || Application.isLoadingLevel || EditorApplication.isPlayingOrWillChangePlaymode;
			#endif
			return false;
		}
		public static bool IsPlaying(){
			#if UNITY_EDITOR
			return Application.isPlaying || Utility.IsBusy();
			#endif
			return Application.isPlaying;
		}
		//============================
		// Proxy - Undo
		//============================
		public static void RecordObject(UnityObject target,string name){
			#if UNITY_EDITOR
			Undo.RecordObject(target,name);
			#endif
		}
		public static void RegisterCompleteObjectUndo(UnityObject target,string name){
			#if UNITY_EDITOR
			Undo.RegisterCompleteObjectUndo(target,name);
			#endif
		}
		//============================
		// Proxy - Other
		//============================
		public static void UpdateSelection(){
			#if UNITY_EDITOR
			var targets = Selection.objects;
			var focus = GUI.GetNameOfFocusedControl();
			if(targets.Length > 0){
				Selection.activeObject = null;
				Utility.DelayCall(()=>{
					Selection.objects = targets;
					EditorGUI.FocusTextInControl(focus);
					GUI.FocusControl(focus);
				},0.05f);
			}
			#endif
		}
		public static void RebuildInspectors(){
			#if UNITY_EDITOR
			Type inspectorType = Utility.GetInternalType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				var tracker = windows[index].CallMethod<ActiveEditorTracker>("GetTracker");
				tracker.ForceRebuild();
			}
			#endif
		}
		public static void ShowInspectors(){
			#if UNITY_EDITOR
			Type inspectorType = Utility.GetInternalType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				var tracker = windows[index].CallMethod<ActiveEditorTracker>("GetTracker");
				for(int editorIndex=0;editorIndex<tracker.activeEditors.Length;++editorIndex){
					tracker.SetVisible(editorIndex,1);
				}
			}
			#endif
		}
		public static void RepaintInspectors(){
			#if UNITY_EDITOR
			Type inspectorType = Utility.GetInternalType("InspectorWindow");
			inspectorType.CallMethod("RepaintAllInspectors");
			#endif
		}
		public static void RepaintAll(){
			#if UNITY_EDITOR
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			#endif
		}
		public static void RepaintGameView(){
			#if UNITY_EDITOR
			Type viewType = Utility.GetInternalType("GameView");
			EditorWindow gameview = EditorWindow.GetWindow(viewType);
			gameview.Repaint();
			#endif
		}
		public static void RepaintSceneView(){
			#if UNITY_EDITOR
			if(SceneView.lastActiveSceneView != null){
				SceneView.lastActiveSceneView.Repaint();
			}
			#endif
		}
		public static void ClearDirty(){
			#if UNITY_EDITOR
			Utility.delayedDirty.Clear();
			#endif
		}
		public static void SetDirty(UnityObject target,bool delayed=false,bool forced=false){
			#if UNITY_EDITOR
			if(Application.isPlaying){return;}
			if(target.IsNull()){return;}
			if(!forced && target.GetPrefab().IsNull()){return;}
			if(delayed){
				if(!Utility.delayedDirty.Contains(target)){
					Event.AddLimited("On Enter Play",()=>Utility.SetDirty(target),1);
					Event.AddLimited("On Enter Play",Utility.ClearDirty,1);
					Utility.delayedDirty.AddNew(target);
				}
				return;
			}
			EditorUtility.SetDirty(target);
			//Utility.UpdatePrefab(target);
			#endif
		}
		public static void SetAssetDirty(UnityObject target){
			#if UNITY_EDITOR
			string path = AssetDatabase.GetAssetPath(target);
			UnityObject asset = AssetDatabase.LoadMainAssetAtPath(path);
			Utility.SetDirty(asset,false,true);
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
			return (bool)Utility.GetInternalType("ComponentUtility").CallMethod("MoveComponentUp",component.AsArray());
			#endif
			return false;
		}
		public static bool MoveComponentDown(Component component){
			#if UNITY_EDITOR
			return (bool)Utility.GetInternalType("ComponentUtility").CallMethod("MoveComponentDown",component.AsArray());
			#endif
			return false;
		}
	}
}