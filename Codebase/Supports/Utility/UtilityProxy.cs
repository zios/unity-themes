#pragma warning disable 0162
#pragma warning disable 0618
using System;
using System.Collections;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	using Events;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	public static partial class Utility{
		//============================
		// EditorUtility
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
		// AssetDatabase
		//============================
		public static bool IsAsset(UnityObject target){
			#if UNITY_EDITOR
			return !AssetDatabase.GetAssetPath(target).IsEmpty();
			#endif
			return false;
		}
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
		// PrefabUtility
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
		// EditorApplication
		//============================
		public static bool IsPaused(){
			#if UNITY_EDITOR
			return EditorApplication.isPaused;
			#endif
			return false;
		}
		public static bool IsBusy(){
			#if UNITY_EDITOR
			return EventDetector.loading || Application.isLoadingLevel || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating;
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
		// Undo
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
		// Other
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
		public static void RebuildAll(){
			#if UNITY_EDITOR
			var windows = Locate.GetAssets<EditorWindow>();
			foreach(var window in windows){
				if(windows.IsNull()){continue;}
				var tracker = window.GetVariable<ActiveEditorTracker>("m_Tracker");
				if(tracker == null){continue;}
				tracker.ForceRebuild();
			}
			#endif
		}
		public static void RebuildInspectors(){
			#if UNITY_EDITOR
			//typeof(EditorUtility).CallMethod("ForceRebuildInspectors");
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				if(windows[index].IsNull()){continue;}
				var tracker = windows[index].GetVariable<ActiveEditorTracker>("m_Tracker");
				if(tracker == null){continue;}
				tracker.ForceRebuild();
			}
			#endif
		}
		public static void ShowInspectors(){
			#if UNITY_EDITOR
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				var tracker = windows[index].GetVariable<ActiveEditorTracker>("m_Tracker");
				if(tracker == null){continue;}
				for(int editorIndex=0;editorIndex<tracker.activeEditors.Length;++editorIndex){
					tracker.SetVisible(editorIndex,1);
				}
			}
			#endif
		}
		public static void RepaintInspectors(){
			#if UNITY_EDITOR
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			inspectorType.CallMethod("RepaintAllInspectors");
			#endif
		}
		public static void RepaintToolbar(){
			#if UNITY_EDITOR
			Utility.GetUnityType("Toolbar").CallMethod("RepaintToolbar");
			#endif
		}
		public static void RepaintAll(){
			#if UNITY_EDITOR
			//foreach(var window in Locate.GetAssets<EditorWindow>()){window.Repaint();}
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			//typeof(EditorApplication).CallMethod("RequestRepaintAllViews");
			//Utility.GetUnityType("InspectorWindow").CallMethod("RepaintAllInspectors");
			Utility.RepaintToolbar();
			#endif
		}
		public static void RepaintGameView(){
			#if UNITY_EDITOR
			Type viewType = Utility.GetUnityType("GameView");
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
			#if UNITY_5_3_OR_NEWER
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
			#else
			EditorApplication.MarkSceneDirty();
			#endif
			//Utility.UpdatePrefab(target);
			#endif
		}
		public static void SetAssetDirty(UnityObject target){
			#if UNITY_EDITOR
			string path = FileManager.GetPath(target);
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
			return Unsupported.GetLocalIdentifierInFile(instanceID);
			#endif
			return 0;
		}
		public static bool MoveComponentUp(Component component){
			#if UNITY_EDITOR
			return (bool)Utility.GetUnityType("ComponentUtility").CallMethod("MoveComponentUp",component.AsArray());
			#endif
			return false;
		}
		public static bool MoveComponentDown(Component component){
			#if UNITY_EDITOR
			return (bool)Utility.GetUnityType("ComponentUtility").CallMethod("MoveComponentDown",component.AsArray());
			#endif
			return false;
		}
		public static void LoadScene(string name){
			#if UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.LoadScene(name);
			#else
			Application.LoadLevel(name);
			#endif
		}
	}
}